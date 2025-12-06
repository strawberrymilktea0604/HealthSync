export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface ChatResponse {
  response: string;
  timestamp: string;
  messageId: string;
}

export interface ChatRequest {
  question: string;
}
