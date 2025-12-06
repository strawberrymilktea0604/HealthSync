import React, { useState, useEffect, useRef } from 'react';
import { Send, Bot, User, RefreshCw } from 'lucide-react';
import { chatService } from '../services/chatService';
import { ChatMessage } from '../types/chat';

const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    loadChatHistory();
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const loadChatHistory = async () => {
    setIsLoading(true);
    try {
      const history = await chatService.getChatHistory();
      setMessages(history);
    } catch (error) {
      console.error('Error loading chat history:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const question = inputMessage.trim();
    if (!question || isSending) return;

    // Add user message immediately
    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: question,
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInputMessage('');
    setIsSending(true);

    try {
      const response = await chatService.sendMessage(question);
      
      const aiMessage: ChatMessage = {
        id: response.messageId,
        role: 'assistant',
        content: response.response,
        createdAt: response.timestamp,
      };

      setMessages((prev) => [...prev, aiMessage]);
    } catch (error: unknown) {
      console.error('Error sending message:', error);
      alert(error.response?.data?.message || 'Không thể gửi tin nhắn. Vui lòng thử lại.');
    } finally {
      setIsSending(false);
    }
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className="flex flex-col h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-[#D4C5A9] text-white px-6 py-4 shadow-md">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Bot className="w-8 h-8" />
            <div>
              <h1 className="text-xl font-bold">HealthBot 💪</h1>
              <p className="text-sm text-white/80">Trợ lý sức khỏe AI của bạn</p>
            </div>
          </div>
          <button
            onClick={loadChatHistory}
            className="p-2 hover:bg-white/10 rounded-full transition"
            disabled={isLoading}
          >
            <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Welcome Banner */}
      <div className="bg-[#D4C5A9]/20 border-b border-[#D4C5A9] px-6 py-4">
        <div className="max-w-6xl mx-auto text-center">
          <p className="text-lg font-semibold">🤖 Xin chào! Tôi là HealthSync Bot</p>
          <p className="text-sm text-gray-600">
            Hãy hỏi tôi về dinh dưỡng, luyện tập và sức khỏe của bạn
          </p>
        </div>
      </div>

      {/* Messages Container */}
      <div className="flex-1 overflow-y-auto px-6 py-6">
        <div className="max-w-4xl mx-auto space-y-4">
          {isLoading ? (
            <div className="flex justify-center items-center h-64">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#D4C5A9]"></div>
            </div>
          ) : messages.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-64 text-gray-400">
              <Bot className="w-20 h-20 mb-4" />
              <p className="text-lg">Bắt đầu cuộc trò chuyện</p>
            </div>
          ) : (
            messages.map((message) => (
              <div
                key={message.id}
                className={`flex gap-3 ${
                  message.role === 'user' ? 'justify-end' : 'justify-start'
                }`}
              >
                {message.role === 'assistant' && (
                  <div className="w-10 h-10 rounded-full bg-[#D4C5A9] flex items-center justify-center flex-shrink-0">
                    <Bot className="w-5 h-5 text-white" />
                  </div>
                )}
                
                <div
                  className={`max-w-2xl rounded-2xl px-4 py-3 ${
                    message.role === 'user'
                      ? 'bg-[#D4C5A9] text-white'
                      : 'bg-white border border-gray-200'
                  }`}
                >
                  <p className="text-sm whitespace-pre-wrap">{message.content}</p>
                  <p
                    className={`text-xs mt-1 ${
                      message.role === 'user' ? 'text-white/70' : 'text-gray-500'
                    }`}
                  >
                    {formatTime(message.createdAt)}
                  </p>
                </div>

                {message.role === 'user' && (
                  <div className="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center flex-shrink-0">
                    <User className="w-5 h-5 text-white" />
                  </div>
                )}
              </div>
            ))
          )}
          
          {isSending && (
            <div className="flex gap-3 justify-start">
              <div className="w-10 h-10 rounded-full bg-[#D4C5A9] flex items-center justify-center flex-shrink-0">
                <Bot className="w-5 h-5 text-white" />
              </div>
              <div className="bg-white border border-gray-200 rounded-2xl px-4 py-3">
                <div className="flex gap-1">
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"></div>
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce delay-100"></div>
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce delay-200"></div>
                </div>
              </div>
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </div>
      </div>

      {/* Input Area */}
      <div className="bg-white border-t border-gray-200 px-6 py-4">
        <form onSubmit={handleSendMessage} className="max-w-4xl mx-auto">
          <div className="flex gap-3">
            <input
              type="text"
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              placeholder="Nhập câu hỏi của bạn..."
              className="flex-1 px-4 py-3 border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-[#D4C5A9] focus:border-transparent"
              disabled={isSending}
            />
            <button
              type="submit"
              disabled={isSending || !inputMessage.trim()}
              className="w-12 h-12 bg-[#D4C5A9] text-white rounded-full flex items-center justify-center hover:bg-[#C4B599] transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Send className="w-5 h-5" />
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ChatScreen;
