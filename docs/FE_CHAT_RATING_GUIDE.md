# ??? FE Guide: Chat Real-time & ?ánh giá ng??i bán

> **UC 1 — Chat / Messaging:** H? th?ng nh?n tin real-time gi?a Buyer & Seller qua SignalR + REST API fallback.
>
> **UC 2 — Rate Seller:** ?ánh giá ng??i bán sau khi hoàn thành ??n hàng — hi?n th? sao, bình lu?n, ?i?m uy tín.
>
> **Base URL:** `http://localhost:5xxx` (ki?m tra port BE)
>
> **SignalR Hub URL:** `http://localhost:5xxx/chathub`
>
> **Stack FE:** React + Vite + TypeScript + Axios + `@microsoft/signalr`

---

## ?? Cài ??t th? vi?n SignalR cho FE

```bash
npm install @microsoft/signalr
```

> **L?u ý:** ?ây là th? vi?n **client** chính th?c c?a Microsoft cho SignalR.
> Không c?n cài thêm `socket.io` hay th? vi?n WebSocket nào khác.

---

## ?? API Endpoint Summary

### UC 1 — Chat / Messaging (?? T?t c? c?n Auth)

| Method | Endpoint | Mô t? |
|--------|----------|--------|
| `POST` | `/api/messages` | G?i tin nh?n (REST fallback) |
| `GET` | `/api/messages/conversations` | L?y danh sách h?i tho?i |
| `GET` | `/api/messages/conversations/{otherUserId}` | L?y l?ch s? chat v?i 1 user |
| `PATCH` | `/api/messages/conversations/{otherUserId}/read` | ?ánh d?u ?ã ??c |
| `GET` | `/api/messages/unread-count` | ??m s? tin nh?n ch?a ??c |

**SignalR Hub:** `/chathub`

| Hub Method (FE g?i) | Params | Mô t? |
|----------------------|--------|--------|
| `SendMessage` | `SendMessageDto` | G?i tin nh?n real-time |
| `MarkAsRead` | `otherUserId: number` | ?ánh d?u ?ã ??c real-time |

| Hub Event (FE l?ng nghe) | Data | Mô t? |
|---------------------------|------|--------|
| `ReceiveMessage` | `MessageDto` | Nh?n tin nh?n m?i |
| `MessagesRead` | `userId: number` | ??i ph??ng ?ã ??c tin nh?n |

### UC 2 — Rate Seller

| Method | Endpoint | Auth | Mô t? |
|--------|----------|------|--------|
| `POST` | `/api/ratings` | ?? | T?o ?ánh giá cho seller (sau order hoàn thành) |
| `GET` | `/api/ratings/seller/{sellerId}` | ? | L?y danh sách ?ánh giá c?a seller |
| `GET` | `/api/ratings/seller/{sellerId}/stats` | ? | L?y th?ng kê uy tín seller (avg rating, t?ng reviews) |
| `GET` | `/api/ratings/order/{orderId}/check` | ?? | Ki?m tra ?ã ?ánh giá order này ch?a |

---

## ?? UC 1: CHAT REAL-TIME

### 1.1. Ki?n trúc t?ng quan

```
????????????????   SignalR WebSocket    ????????????????
?   Buyer FE   ? ?????????????????????? ?   ChatHub    ?
????????????????                        ?   (BE)       ?
                                        ????????????????
????????????????   SignalR WebSocket           ?
?  Seller FE   ? ?????????????????????? ????????
????????????????
         ?
         ?  REST API (fallback / load history)
         ?
   /api/messages/*
```

**Lu?ng ho?t ??ng:**
1. FE k?t n?i SignalR Hub v?i JWT token ? BE t? ??ng join user vào group theo userId
2. G?i tin nh?n: FE g?i Hub method `SendMessage` ? BE l?u DB ? broadcast `ReceiveMessage` cho c? 2 bên
3. Load l?ch s?: FE g?i REST `GET /api/messages/conversations/{otherUserId}`
4. ?ánh d?u ?ã ??c: FE g?i Hub method `MarkAsRead` ? BE c?p nh?t DB ? notify ??i ph??ng

### 1.2. REST APIs — Chi ti?t

#### G?i tin nh?n — `POST /api/messages`

> **Dùng khi:** SignalR ch?a k?t n?i ho?c fallback khi WebSocket b? l?i.
> **?u tiên:** Dùng SignalR `SendMessage` thay vì REST.

**Request Body:**
```json
{
  "receiverId": 5,
  "listingId": 10,
  "content": "Xe này còn không b?n?"
}
```

| Field | Type | Required | Mô t? |
|-------|------|----------|--------|
| `receiverId` | number | ? | ID ng??i nh?n |
| `listingId` | number | ? | ID listing liên quan (t?o context cho chat session) |
| `content` | string | ? | N?i dung tin nh?n |

**Response `200 OK`:**
```json
{
  "messageId": 42,
  "senderId": 3,
  "senderName": "buyer_user",
  "receiverId": 5,
  "content": "Xe này còn không b?n?",
  "isRead": false,
  "sentAt": "2025-06-25T14:30:00Z"
}
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Sender not found"` | Token không h?p l? |
| `"Receiver not found"` | receiverId không t?n t?i |

#### Danh sách h?i tho?i — `GET /api/messages/conversations`

**Response `200 OK`:**
```json
[
  {
    "otherUserId": 5,
    "otherUserName": "seller_john",
    "lastMessage": "Ok b?n, xe v?n còn nhé",
    "lastMessageAt": "2025-06-25T14:35:00Z",
    "unreadCount": 2
  },
  {
    "otherUserId": 8,
    "otherUserName": "buyer_jane",
    "lastMessage": "C?m ?n b?n!",
    "lastMessageAt": "2025-06-24T10:00:00Z",
    "unreadCount": 0
  }
]
```

> Danh sách ?ã ???c s?p x?p theo `lastMessageAt` gi?m d?n (cu?c h?i tho?i m?i nh?t lên ??u).

#### L?ch s? chat — `GET /api/messages/conversations/{otherUserId}`

**Response `200 OK`:**
```json
[
  {
    "messageId": 40,
    "senderId": 3,
    "senderName": "buyer_user",
    "receiverId": 5,
    "content": "Xe này còn không b?n?",
    "isRead": true,
    "sentAt": "2025-06-25T14:30:00Z"
  },
  {
    "messageId": 41,
    "senderId": 5,
    "senderName": "seller_john",
    "receiverId": 3,
    "content": "Ok b?n, xe v?n còn nhé",
    "isRead": false,
    "sentAt": "2025-06-25T14:35:00Z"
  }
]
```

> ?ã s?p x?p theo `sentAt` t?ng d?n (tin c? ? m?i).

#### ?ánh d?u ?ã ??c — `PATCH /api/messages/conversations/{otherUserId}/read`

**Response `200 OK`:**
```json
{ "message": "Success" }
```

#### S? tin nh?n ch?a ??c — `GET /api/messages/unread-count`

**Response `200 OK`:** `number` (VD: `5`)

---

### ?? FE Implementation — Chat

#### TypeScript Interfaces

```typescript
// src/types/chat.ts

interface SendMessageDto {
  receiverId: number;
  listingId?: number;
  content: string;
}

interface MessageDto {
  messageId: number;
  senderId: number;
  senderName: string;
  receiverId: number;
  content: string;
  isRead?: boolean;
  sentAt?: string;
}

interface ConversationDto {
  otherUserId: number;
  otherUserName: string;
  lastMessage: string;
  lastMessageAt?: string;
  unreadCount: number;
}
```

#### SignalR Connection Hook

```typescript
// src/hooks/useSignalR.ts
import { useEffect, useRef, useState, useCallback } from 'react';
import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HubConnectionState,
} from '@microsoft/signalr';

const CHATHUB_URL = import.meta.env.VITE_API_URL + '/chathub';

export const useSignalR = (token: string | null) => {
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl(CHATHUB_URL, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // T? ??ng reconnect
      .configureLogging(LogLevel.Information)
      .build();

    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));
    connection.onclose(() => setIsConnected(false));

    connection
      .start()
      .then(() => {
        console.log('SignalR connected');
        setIsConnected(true);
      })
      .catch((err) => {
        console.error('SignalR connection error:', err);
        setIsConnected(false);
      });

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [token]);

  // G?i tin nh?n real-time
  const sendMessage = useCallback(async (dto: SendMessageDto) => {
    const conn = connectionRef.current;
    if (conn?.state === HubConnectionState.Connected) {
      await conn.invoke('SendMessage', dto);
    }
  }, []);

  // ?ánh d?u ?ã ??c real-time
  const markAsRead = useCallback(async (otherUserId: number) => {
    const conn = connectionRef.current;
    if (conn?.state === HubConnectionState.Connected) {
      await conn.invoke('MarkAsRead', otherUserId);
    }
  }, []);

  // L?ng nghe event
  const onReceiveMessage = useCallback(
    (callback: (message: MessageDto) => void) => {
      connectionRef.current?.on('ReceiveMessage', callback);
      return () => connectionRef.current?.off('ReceiveMessage', callback);
    },
    []
  );

  const onMessagesRead = useCallback(
    (callback: (userId: number) => void) => {
      connectionRef.current?.on('MessagesRead', callback);
      return () => connectionRef.current?.off('MessagesRead', callback);
    },
    []
  );

  return {
    isConnected,
    sendMessage,
    markAsRead,
    onReceiveMessage,
    onMessagesRead,
    connection: connectionRef.current,
  };
};
```

#### Chat Service (REST fallback)

```typescript
// src/services/chatService.ts
import api from './api';

export const chatService = {
  send: (dto: SendMessageDto) =>
    api.post<MessageDto>('/api/messages', dto),

  getConversations: () =>
    api.get<ConversationDto[]>('/api/messages/conversations'),

  getConversation: (otherUserId: number) =>
    api.get<MessageDto[]>(`/api/messages/conversations/${otherUserId}`),

  markAsRead: (otherUserId: number) =>
    api.patch(`/api/messages/conversations/${otherUserId}/read`),

  getUnreadCount: () =>
    api.get<number>('/api/messages/unread-count'),
};
```

#### Chat Context (Global State)

```tsx
// src/contexts/ChatContext.tsx
import { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useSignalR } from '../hooks/useSignalR';
import { chatService } from '../services/chatService';

interface ChatContextType {
  conversations: ConversationDto[];
  totalUnread: number;
  sendMessage: (dto: SendMessageDto) => Promise<void>;
  markAsRead: (otherUserId: number) => Promise<void>;
  refreshConversations: () => Promise<void>;
  isConnected: boolean;
  onReceiveMessage: (callback: (msg: MessageDto) => void) => (() => void) | undefined;
  onMessagesRead: (callback: (userId: number) => void) => (() => void) | undefined;
}

const ChatContext = createContext<ChatContextType | null>(null);

export const ChatProvider = ({ children }: { children: ReactNode }) => {
  const { token, isAuthenticated } = useAuth();
  const signalR = useSignalR(isAuthenticated ? token : null);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [totalUnread, setTotalUnread] = useState(0);

  // Load danh sách h?i tho?i khi authenticated
  const refreshConversations = async () => {
    if (!isAuthenticated) return;
    try {
      const [convRes, unreadRes] = await Promise.all([
        chatService.getConversations(),
        chatService.getUnreadCount(),
      ]);
      setConversations(convRes.data);
      setTotalUnread(unreadRes.data);
    } catch {
      // ignore
    }
  };

  useEffect(() => {
    refreshConversations();
  }, [isAuthenticated]);

  // L?ng nghe tin nh?n m?i ? c?p nh?t conversations & unread
  useEffect(() => {
    const cleanup = signalR.onReceiveMessage(() => {
      refreshConversations(); // Reload danh sách khi có tin m?i
    });
    return cleanup;
  }, [signalR.onReceiveMessage]);

  // L?ng nghe messagesRead ? c?p nh?t unread
  useEffect(() => {
    const cleanup = signalR.onMessagesRead(() => {
      refreshConversations();
    });
    return cleanup;
  }, [signalR.onMessagesRead]);

  const sendMessage = async (dto: SendMessageDto) => {
    if (signalR.isConnected) {
      await signalR.sendMessage(dto);
    } else {
      // Fallback REST
      await chatService.send(dto);
    }
  };

  const markAsRead = async (otherUserId: number) => {
    if (signalR.isConnected) {
      await signalR.markAsRead(otherUserId);
    } else {
      await chatService.markAsRead(otherUserId);
    }
    await refreshConversations();
  };

  return (
    <ChatContext.Provider
      value={{
        conversations,
        totalUnread,
        sendMessage,
        markAsRead,
        refreshConversations,
        isConnected: signalR.isConnected,
        onReceiveMessage: signalR.onReceiveMessage,
        onMessagesRead: signalR.onMessagesRead,
      }}
    >
      {children}
    </ChatContext.Provider>
  );
};

export const useChat = () => {
  const ctx = useContext(ChatContext);
  if (!ctx) throw new Error('useChat must be used within ChatProvider');
  return ctx;
};
```

#### Conversation List Page

```tsx
// src/pages/ConversationsPage.tsx
import { Link } from 'react-router-dom';
import { useChat } from '../contexts/ChatContext';

const ConversationsPage = () => {
  const { conversations } = useChat();

  const formatTime = (dateStr?: string) => {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'V?a xong';
    if (diffMin < 60) return `${diffMin} phút tr??c`;
    const diffHours = Math.floor(diffMin / 60);
    if (diffHours < 24) return `${diffHours} gi? tr??c`;
    return date.toLocaleDateString('vi-VN');
  };

  return (
    <div className="conversations-page">
      <h1>Tin nh?n</h1>
      {conversations.length === 0 ? (
        <p>Ch?a có cu?c trò chuy?n nào</p>
      ) : (
        <div className="conversation-list">
          {conversations.map((conv) => (
            <Link
              key={conv.otherUserId}
              to={`/messages/${conv.otherUserId}`}
              className={`conversation-item ${conv.unreadCount > 0 ? 'unread' : ''}`}
            >
              <div className="conv-avatar">
                {conv.otherUserName.charAt(0).toUpperCase()}
              </div>
              <div className="conv-body">
                <div className="conv-header">
                  <span className="conv-name">{conv.otherUserName}</span>
                  <span className="conv-time">{formatTime(conv.lastMessageAt)}</span>
                </div>
                <div className="conv-preview">
                  <span className="conv-last-msg">{conv.lastMessage}</span>
                  {conv.unreadCount > 0 && (
                    <span className="unread-badge">{conv.unreadCount}</span>
                  )}
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
};
```

#### Chat Room Component

```tsx
// src/pages/ChatRoomPage.tsx
import { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useChat } from '../contexts/ChatContext';
import { chatService } from '../services/chatService';

const ChatRoomPage = () => {
  const { otherUserId } = useParams<{ otherUserId: string }>();
  const { user } = useAuth();
  const { sendMessage, markAsRead, onReceiveMessage } = useChat();

  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const otherUid = Number(otherUserId);

  // Load l?ch s? chat
  useEffect(() => {
    const load = async () => {
      try {
        const res = await chatService.getConversation(otherUid);
        setMessages(res.data);
        // ?ánh d?u ?ã ??c khi m? cu?c chat
        await markAsRead(otherUid);
      } catch {
        setMessages([]);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [otherUid]);

  // L?ng nghe tin nh?n real-time
  useEffect(() => {
    const cleanup = onReceiveMessage((msg: MessageDto) => {
      // Ch? thêm tin nh?n thu?c cu?c chat hi?n t?i
      if (msg.senderId === otherUid || msg.receiverId === otherUid) {
        setMessages((prev) => [...prev, msg]);
        // Auto ?ánh d?u ?ã ??c n?u ?ang ? trong chat room
        if (msg.senderId === otherUid) {
          markAsRead(otherUid);
        }
      }
    });
    return cleanup;
  }, [otherUid, onReceiveMessage]);

  // Auto scroll xu?ng cu?i
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // G?i tin nh?n
  const handleSend = async () => {
    const content = input.trim();
    if (!content) return;

    setInput('');
    await sendMessage({
      receiverId: otherUid,
      content,
    });
  };

  // G?i b?ng Enter
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  if (loading) return <div>?ang t?i...</div>;

  return (
    <div className="chat-room">
      {/* Messages area */}
      <div className="messages-container">
        {messages.map((msg) => {
          const isMine = msg.senderId === user?.userId;
          return (
            <div
              key={msg.messageId}
              className={`message ${isMine ? 'mine' : 'theirs'}`}
            >
              <div className="message-content">{msg.content}</div>
              <div className="message-meta">
                <span className="message-time">
                  {msg.sentAt
                    ? new Date(msg.sentAt).toLocaleTimeString('vi-VN', {
                        hour: '2-digit',
                        minute: '2-digit',
                      })
                    : ''}
                </span>
                {isMine && (
                  <span className="message-status">
                    {msg.isRead ? '??' : '?'}
                  </span>
                )}
              </div>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      {/* Input area */}
      <div className="chat-input">
        <textarea
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Nh?p tin nh?n..."
          rows={1}
        />
        <button onClick={handleSend} disabled={!input.trim()}>
          G?i
        </button>
      </div>
    </div>
  );
};
```

#### Nút Chat t? trang Bike Detail

```tsx
// Trong BikeDetailPage.tsx — Thêm nút "Chat v?i ng??i bán"
import { useNavigate } from 'react-router-dom';

// Trong render, bên d??i seller-info:
const handleChat = () => {
  if (!isAuthenticated) return navigate('/login');
  navigate(`/messages/${listing.sellerId}`);
};

// Không hi?n th? n?u là ch? bài ??ng
{!isOwner && (
  <button onClick={handleChat} className="btn-chat">
    ?? Chat v?i ng??i bán
  </button>
)}
```

#### G?i tin nh?n kèm context Listing

```tsx
// Khi b?m "Chat v?i ng??i bán" t? trang detail, g?i kèm listingId
const handleChatAboutListing = async () => {
  if (!isAuthenticated) return navigate('/login');
  
  // G?i tin nh?n ??u tiên kèm context listing
  await sendMessage({
    receiverId: listing.sellerId,
    listingId: listing.listingId,
    content: `Xin chào, tôi quan tâm ??n "${listing.title}". Xe này còn không?`,
  });
  
  navigate(`/messages/${listing.sellerId}`);
};
```

#### Unread Badge ? Header

```tsx
// src/components/ChatBadge.tsx
import { Link } from 'react-router-dom';
import { useChat } from '../contexts/ChatContext';

const ChatBadge = () => {
  const { totalUnread } = useChat();

  return (
    <Link to="/messages" className="chat-badge">
      ??
      {totalUnread > 0 && <span className="badge-count">{totalUnread}</span>}
    </Link>
  );
};
```

---

## ? UC 2: ?ÁNH GIÁ NG??I BÁN

### 2.1. T?o ?ánh giá — `POST /api/ratings` (?? Auth)

> Ch? Buyer c?a order ?ã hoàn thành (status = 4) m?i ???c ?ánh giá.
> M?i order ch? ???c ?ánh giá **1 l?n**.

**Request Body:**
```json
{
  "orderId": 15,
  "rating": 5,
  "comment": "Xe ??p, seller giao hàng nhanh, ?úng mô t?!"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `orderId` | number | ? | ID order ?ã hoàn thành |
| `rating` | number | ? | 1–5 sao |
| `comment` | string | ? | T?i ?a 1000 ký t? |

**Response `200 OK`:**
```json
{
  "feedbackId": 7,
  "orderId": 15,
  "rating": 5,
  "comment": "Xe ??p, seller giao hàng nhanh, ?úng mô t?!",
  "fromUserName": "buyer_user",
  "createdAt": "2025-06-25T16:00:00Z"
}
```

**Response `400`:**

| Error | Nguyên nhân |
|-------|-------------|
| `"Order not found"` | OrderId không t?n t?i |
| `"Only the buyer can rate"` | User không ph?i buyer c?a order |
| `"Order must be completed first"` | Order ch?a hoàn thành (status ? 4) |
| `"This order has already been rated"` | ?ã ?ánh giá r?i |
| `"Order has no items"` | Order không có detail |
| `"Listing not found"` | Listing liên quan không t?n t?i |

### 2.2. Danh sách ?ánh giá — `GET /api/ratings/seller/{sellerId}` (Public)

**Response `200 OK`:**
```json
[
  {
    "feedbackId": 7,
    "orderId": 15,
    "rating": 5,
    "comment": "Xe ??p, giao nhanh!",
    "fromUserName": "buyer_user",
    "createdAt": "2025-06-25T16:00:00Z"
  },
  {
    "feedbackId": 3,
    "orderId": 8,
    "rating": 4,
    "comment": "Xe t?t, nh?ng giao h?i ch?m",
    "fromUserName": "jane_doe",
    "createdAt": "2025-06-20T10:00:00Z"
  }
]
```

> ?ã s?p x?p theo `createdAt` gi?m d?n (m?i nh?t lên ??u).

### 2.3. Th?ng kê uy tín seller — `GET /api/ratings/seller/{sellerId}/stats` (Public)

**Response `200 OK`:**
```json
{
  "sellerId": 5,
  "sellerName": "seller_john",
  "averageRating": 4.3,
  "totalReviews": 12,
  "ratingDistribution": {
    "1": 0,
    "2": 1,
    "3": 2,
    "4": 4,
    "5": 5
  }
}
```

| Field | Mô t? |
|-------|--------|
| `averageRating` | ?i?m trung bình (0–5), làm tròn 1 ch? s? th?p phân |
| `totalReviews` | T?ng s? ?ánh giá (k? c? không có rating) |
| `ratingDistribution` | Phân b? s? l??ng theo t?ng m?c sao (1–5) |

### 2.4. Ki?m tra ?ã ?ánh giá — `GET /api/ratings/order/{orderId}/check` (?? Auth)

**Response `200 OK`:** `true` ho?c `false`

> Dùng ?? FE quy?t ??nh hi?n th? form ?ánh giá hay badge "?ã ?ánh giá".

---

### ?? FE Implementation — Rating

#### TypeScript Interfaces

```typescript
// src/types/rating.ts

interface CreateRatingDto {
  orderId: number;
  rating?: number;
  comment?: string;
}

interface RatingDto {
  feedbackId: number;
  orderId: number;
  rating?: number;
  comment?: string;
  fromUserName: string;
  createdAt?: string;
}

interface SellerStatsDto {
  sellerId: number;
  sellerName: string;
  averageRating: number;
  totalReviews: number;
  ratingDistribution: Record<string, number>;
}
```

#### Rating Service

```typescript
// src/services/ratingService.ts
import api from './api';

export const ratingService = {
  create: (dto: CreateRatingDto) =>
    api.post<RatingDto>('/api/ratings', dto),

  getBySeller: (sellerId: number) =>
    api.get<RatingDto[]>(`/api/ratings/seller/${sellerId}`),

  getSellerStats: (sellerId: number) =>
    api.get<SellerStatsDto>(`/api/ratings/seller/${sellerId}/stats`),

  hasRatedOrder: (orderId: number) =>
    api.get<boolean>(`/api/ratings/order/${orderId}/check`),
};
```

#### Star Rating Input Component

```tsx
// src/components/StarRatingInput.tsx
import { useState } from 'react';

interface StarRatingInputProps {
  value: number;
  onChange: (value: number) => void;
  size?: number;
}

const StarRatingInput = ({ value, onChange, size = 24 }: StarRatingInputProps) => {
  const [hovered, setHovered] = useState(0);

  return (
    <div className="star-rating-input">
      {[1, 2, 3, 4, 5].map((star) => (
        <span
          key={star}
          className={`star ${star <= (hovered || value) ? 'active' : ''}`}
          style={{ fontSize: size, cursor: 'pointer' }}
          onClick={() => onChange(star)}
          onMouseEnter={() => setHovered(star)}
          onMouseLeave={() => setHovered(0)}
        >
          {star <= (hovered || value) ? '?' : '?'}
        </span>
      ))}
    </div>
  );
};
```

#### Star Rating Display Component

```tsx
// src/components/StarRatingDisplay.tsx
interface StarRatingDisplayProps {
  rating: number;
  size?: number;
  showValue?: boolean;
}

const StarRatingDisplay = ({ rating, size = 16, showValue = true }: StarRatingDisplayProps) => {
  const fullStars = Math.floor(rating);
  const hasHalf = rating - fullStars >= 0.5;

  return (
    <div className="star-rating-display">
      {[1, 2, 3, 4, 5].map((star) => (
        <span key={star} style={{ fontSize: size, color: '#f59e0b' }}>
          {star <= fullStars ? '?' : star === fullStars + 1 && hasHalf ? '?' : '?'}
        </span>
      ))}
      {showValue && <span className="rating-value">{rating.toFixed(1)}</span>}
    </div>
  );
};
```

#### Create Rating Form

```tsx
// src/components/CreateRatingForm.tsx
import { useState } from 'react';
import { ratingService } from '../services/ratingService';

interface Props {
  orderId: number;
  onSuccess: (rating: RatingDto) => void;
}

const CreateRatingForm = ({ orderId, onSuccess }: Props) => {
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (rating === 0) return setError('Vui lòng ch?n s? sao');

    setLoading(true);
    setError('');
    try {
      const res = await ratingService.create({
        orderId,
        rating,
        comment: comment.trim() || undefined,
      });
      onSuccess(res.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'G?i ?ánh giá th?t b?i');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="rating-form">
      <h3>?ánh giá ng??i bán</h3>

      <div className="rating-stars">
        <label>S? sao:</label>
        <StarRatingInput value={rating} onChange={setRating} size={32} />
      </div>

      <div className="rating-comment">
        <label>Nh?n xét (không b?t bu?c):</label>
        <textarea
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Chia s? tr?i nghi?m c?a b?n..."
          maxLength={1000}
          rows={3}
        />
        <span className="char-count">{comment.length}/1000</span>
      </div>

      {error && <p className="error">{error}</p>}

      <button type="submit" disabled={loading || rating === 0}>
        {loading ? '?ang g?i...' : 'G?i ?ánh giá'}
      </button>
    </form>
  );
};
```

#### Rating trong Order History

```tsx
// Trong trang Order Detail ho?c Order History:
import { useState, useEffect } from 'react';
import { ratingService } from '../services/ratingService';

// Ki?m tra ?ã ?ánh giá ch?a khi order.status === 4 (Completed)
const [hasRated, setHasRated] = useState(false);

useEffect(() => {
  if (order?.orderStatus === 4) {
    ratingService.hasRatedOrder(order.orderId)
      .then(res => setHasRated(res.data));
  }
}, [order]);

// Render:
{order.orderStatus === 4 && (
  hasRated ? (
    <span className="rated-badge">? ?ã ?ánh giá</span>
  ) : (
    <CreateRatingForm
      orderId={order.orderId}
      onSuccess={() => setHasRated(true)}
    />
  )
)}
```

#### Seller Stats & Reviews (hi?n th? trên Bike Detail page)

```tsx
// src/components/SellerReputation.tsx
import { useState, useEffect } from 'react';
import { ratingService } from '../services/ratingService';

interface Props {
  sellerId: number;
}

const SellerReputation = ({ sellerId }: Props) => {
  const [stats, setStats] = useState<SellerStatsDto | null>(null);
  const [reviews, setReviews] = useState<RatingDto[]>([]);
  const [showReviews, setShowReviews] = useState(false);

  useEffect(() => {
    ratingService.getSellerStats(sellerId).then(res => setStats(res.data));
  }, [sellerId]);

  const loadReviews = async () => {
    if (reviews.length === 0) {
      const res = await ratingService.getBySeller(sellerId);
      setReviews(res.data);
    }
    setShowReviews(!showReviews);
  };

  if (!stats) return null;

  return (
    <div className="seller-reputation">
      <h3>Uy tín ng??i bán</h3>

      <div className="stats-summary">
        <div className="avg-rating">
          <StarRatingDisplay rating={stats.averageRating} size={24} />
          <span>{stats.averageRating.toFixed(1)} / 5</span>
        </div>
        <span className="total-reviews">{stats.totalReviews} ?ánh giá</span>
      </div>

      {/* Rating distribution bar */}
      <div className="rating-bars">
        {[5, 4, 3, 2, 1].map(star => {
          const count = stats.ratingDistribution[star.toString()] || 0;
          const percentage = stats.totalReviews > 0
            ? Math.round((count / stats.totalReviews) * 100)
            : 0;
          return (
            <div key={star} className="rating-bar-row">
              <span>{star}?</span>
              <div className="bar-container">
                <div className="bar-fill" style={{ width: `${percentage}%` }} />
              </div>
              <span>{count}</span>
            </div>
          );
        })}
      </div>

      {/* Toggle reviews list */}
      <button onClick={loadReviews} className="btn-toggle-reviews">
        {showReviews ? '?n ?ánh giá' : `Xem t?t c? ${stats.totalReviews} ?ánh giá`}
      </button>

      {showReviews && (
        <div className="reviews-list">
          {reviews.length === 0 ? (
            <p>Ch?a có ?ánh giá nào</p>
          ) : (
            reviews.map(review => (
              <div key={review.feedbackId} className="review-item">
                <div className="review-header">
                  <span className="reviewer-name">{review.fromUserName}</span>
                  {review.rating && (
                    <StarRatingDisplay rating={review.rating} size={14} showValue={false} />
                  )}
                  <span className="review-date">
                    {review.createdAt
                      ? new Date(review.createdAt).toLocaleDateString('vi-VN')
                      : ''}
                  </span>
                </div>
                {review.comment && (
                  <p className="review-comment">{review.comment}</p>
                )}
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
};
```

#### Tích h?p SellerReputation vào BikeDetailPage

```tsx
// Trong BikeDetailPage.tsx, thêm d??i seller-info:
<SellerReputation sellerId={listing.sellerId} />
```

---

## ??? React Router Setup

```tsx
// Thêm vào router:
<Route element={<ProtectedRoute />}>
  <Route path="/messages" element={<ConversationsPage />} />
  <Route path="/messages/:otherUserId" element={<ChatRoomPage />} />
</Route>
```

---

## ?? L?U Ý QUAN TR?NG

### 1. SignalR — K?t n?i & Authentication

```
???????????????????????????????????????????????????????????????
? SignalR k?t n?i qua WebSocket v?i JWT token                 ?
?                                                             ?
? URL: /chathub?access_token=<jwt_token>                      ?
?                                                             ?
? ?? Token ???c g?i qua query string (không ph?i header)      ?
? ?? BE t? ??ng xác th?c và join user vào group theo userId    ?
? ?? FE KHÔNG c?n g?i JoinChat — BE t? x? lý ? OnConnected   ?
???????????????????????????????????????????????????????????????
```

### 2. SignalR — Method Signature (QUAN TR?NG)

```typescript
// ? ?ÚNG — Hub method ch? nh?n 1 param là dto object
connection.invoke('SendMessage', {
  receiverId: 5,
  content: 'Hello',
  listingId: 10
});

// ? SAI — Không c?n truy?n senderId (BE t? l?y t? JWT token)
connection.invoke('SendMessage', userId, { ... });
```

```typescript
// ? ?ÚNG — MarkAsRead ch? nh?n otherUserId (number)
connection.invoke('MarkAsRead', 5);

// ? SAI — Không c?n truy?n userId riêng
connection.invoke('MarkAsRead', myUserId, otherUserId);
```

### 3. SignalR — Reconnect & Fallback

- `withAutomaticReconnect` s? t? reconnect khi m?t k?t n?i
- Khi reconnect thành công, BE t? join l?i group (x? lý ? `OnConnectedAsync`)
- N?u WebSocket không kh? d?ng, SignalR t? fallback sang Server-Sent Events ? Long Polling
- FE nên ki?m tra `isConnected` — n?u `false` thì dùng REST API fallback

### 4. Rating — Business Rules

| Rule | Mô t? |
|------|--------|
| Ch? Buyer | Ch? buyer c?a order m?i ???c ?ánh giá |
| Order hoàn thành | Order ph?i có status = 4 (Completed) |
| 1 l?n duy nh?t | M?i order ch? ???c rate 1 l?n |
| Rating 1–5 | S? sao t? 1 ??n 5 (optional — có th? ch? comment) |
| Comment max 1000 | Gi?i h?n 1000 ký t? |
| ?ánh giá cho Seller | Rating l?u vào seller (target) qua listing trong order |

### 5. Order Status Reference

| Value | Label | Có th? ?ánh giá? |
|-------|-------|------------------|
| `1` | Pending | ? |
| `2` | Confirmed | ? |
| `3` | Shipping | ? |
| `4` | **Completed** | ? |
| `5` | Cancelled | ? |
| `6` | Disputed | ? |

### 6. ChatProvider nên wrap toàn b? App

```tsx
// src/App.tsx
import { ChatProvider } from './contexts/ChatContext';

function App() {
  return (
    <AuthProvider>
      <ChatProvider>
        <BrowserRouter>
          <Header /> {/* Ch?a ChatBadge + CartBadge */}
          <Routes>
            {/* ... */}
          </Routes>
        </BrowserRouter>
      </ChatProvider>
    </AuthProvider>
  );
}
```

---

## ? FE Checklist

### UC 1 — Chat Real-time
- [ ] Cài `@microsoft/signalr` (`npm install @microsoft/signalr`)
- [ ] T?o `useSignalR` hook k?t n?i `/chathub` v?i JWT token
- [ ] T?o `ChatContext` qu?n lý global state (conversations, unread count)
- [ ] Wrap `ChatProvider` quanh toàn b? App
- [ ] Trang danh sách h?i tho?i — hi?n th? last message + unread badge
- [ ] Trang chat room — load history + real-time messages
- [ ] Auto scroll xu?ng cu?i khi có tin m?i
- [ ] Hi?n th? tr?ng thái ?ã ??c (? vs ??)
- [ ] G?i tin nh?n b?ng Enter (Shift+Enter xu?ng dòng)
- [ ] Mark as read khi m? cu?c chat
- [ ] Unread badge ? header (?? + s?)
- [ ] Nút "Chat v?i ng??i bán" trên trang Bike Detail
- [ ] Fallback REST API khi SignalR ch?a k?t n?i
- [ ] X? lý reconnect t? ??ng
- [ ] ?n nút chat n?u user là ch? bài ??ng

### UC 2 — ?ánh giá ng??i bán
- [ ] Form ?ánh giá (star input + comment textarea)
- [ ] G?i `POST /api/ratings` khi submit
- [ ] Validate rating 1–5, comment max 1000 chars
- [ ] Ki?m tra ?ã ?ánh giá ch?a (`GET /api/ratings/order/{id}/check`)
- [ ] Hi?n th? "?ã ?ánh giá" badge n?u ?ã rated
- [ ] Ch? hi?n th? form khi order status = 4 (Completed)
- [ ] Component `SellerReputation` — hi?n th? avg rating + distribution bar
- [ ] Component `StarRatingDisplay` — hi?n th? sao (readonly)
- [ ] Component `StarRatingInput` — ch?n sao (interactive)
- [ ] Danh sách reviews v?i reviewer name + date + stars + comment
- [ ] Tích h?p `SellerReputation` vào BikeDetailPage (d??i seller info)
- [ ] Load seller stats 1 l?n khi mount, lazy load reviews khi click
