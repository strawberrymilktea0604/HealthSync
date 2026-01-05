import React, { useState, useEffect, useRef } from 'react';
import { Send, Bot, User, RefreshCw, History, MessageSquarePlus } from 'lucide-react';
import { chatService } from '../services/chatService';
import { ChatMessage } from '../types/chat';
import logo from '../assets/logo.png';

const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Only load chat history if user is logged in
    const userJson = localStorage.getItem('user');
    if (userJson) {
      loadChatHistory();
    } else {
      console.warn('User not logged in - cannot load chat history');
    }
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleNewChat = () => {
    if (messages.length > 0) {
      const confirmNew = window.confirm('Bạn có muốn bắt đầu cuộc trò chuyện mới? Cuộc trò chuyện hiện tại sẽ được lưu vào lịch sử.');
      if (!confirmNew) return;
    }
    setMessages([]);
    setInputMessage('');
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

  const renderChatContent = () => {
    if (isLoading) {
      return (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#D4C5A9]"></div>
        </div>
      );
    }

    if (messages.length === 0) {
      return (
        <div className="flex flex-col items-center justify-center h-full text-gray-400 py-12">
          <div className="w-16 h-16 rounded-full bg-[#D4C5A9]/20 flex items-center justify-center mb-3">
            <Bot className="w-10 h-10 text-[#D4C5A9]" />
          </div>
          <p className="text-base font-medium text-gray-600">Bắt đầu cuộc trò chuyện</p>
          <p className="text-sm text-gray-500 mt-1">Đặt câu hỏi về sức khỏe của bạn</p>
        </div>
      );
    }

    return messages.map((message) => (
      <div
        key={message.id}
        className={`flex gap-2.5 ${message.role === 'user' ? 'justify-end' : 'justify-start'
          }`}
      >
        {message.role === 'assistant' && (
          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-[#D4C5A9] to-[#C4B599] flex items-center justify-center flex-shrink-0 shadow-sm">
            <Bot className="w-4 h-4 text-white" />
          </div>
        )}

        <div
          className={`max-w-[70%] rounded-2xl px-4 py-2.5 ${message.role === 'user'
            ? 'bg-gradient-to-br from-[#D4C5A9] to-[#C4B599] text-white shadow-md'
            : 'bg-white border border-gray-200 shadow-sm'
            }`}
        >
          <p className="text-sm whitespace-pre-wrap leading-relaxed">{message.content}</p>
          <p
            className={`text-xs mt-1.5 ${message.role === 'user' ? 'text-white/80' : 'text-gray-500'
              }`}
          >
            {formatTime(message.createdAt)}
          </p>
        </div>

        {message.role === 'user' && (
          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center flex-shrink-0 shadow-sm">
            <User className="w-4 h-4 text-white" />
          </div>
        )}
      </div>
    ));
  };

  return (
    <div className="flex flex-col h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-[#D4C5A9] to-[#C4B599] text-white px-4 py-3 shadow-lg">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center backdrop-blur-sm">
              <Bot className="w-6 h-6" />
            </div>
            <div>
              <h1 className="text-lg font-bold">HealthBot Assistant</h1>
              <p className="text-xs text-white/90">Trợ lý sức khỏe thông minh</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleNewChat}
              className="p-2 hover:bg-white/20 rounded-lg transition flex items-center gap-1.5 text-sm font-medium"
              title="Tạo cuộc trò chuyện mới"
            >
              <MessageSquarePlus className="w-5 h-5" />
              <span className="hidden sm:inline">Mới</span>
            </button>
            <button
              onClick={() => setShowHistory(!showHistory)}
              className="p-2 hover:bg-white/20 rounded-lg transition flex items-center gap-1.5 text-sm font-medium"
              title="Xem lịch sử chat"
            >
              <History className="w-5 h-5" />
              <span className="hidden sm:inline">Lịch sử</span>
            </button>
            <button
              onClick={loadChatHistory}
              className="p-2 hover:bg-white/20 rounded-lg transition"
              disabled={isLoading}
              title="Làm mới"
            >
              <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
            </button>
          </div>
        </div>
      </div>

      {/* Welcome Banner */}
      <div className="bg-gradient-to-b from-[#D4C5A9]/10 to-transparent border-b border-[#D4C5A9]/30 px-4 py-3">
        <div className="max-w-4xl mx-auto text-center">
          <p className="text-base font-medium flex items-center justify-center gap-2 text-gray-700">
            <img src={logo} alt="HealthSync" className="h-5 inline-block" />{' '}
            Xin chào! Hãy hỏi tôi về sức khỏe của bạn
          </p>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex overflow-hidden">
        {/* Chat History Sidebar */}
        {showHistory && (
          <div className="w-80 bg-white border-r border-gray-200 flex flex-col">
            <div className="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
              <h2 className="font-semibold text-gray-800">Lịch sử trò chuyện</h2>
              <button
                onClick={() => setShowHistory(false)}
                className="text-gray-500 hover:text-gray-700"
              >
                ✕
              </button>
            </div>
            <div className="flex-1 overflow-y-auto p-3 space-y-2">
              {messages.length > 0 ? (
                <div className="space-y-2">
                  <div className="p-3 bg-[#D4C5A9]/10 rounded-lg border border-[#D4C5A9]/30 hover:bg-[#D4C5A9]/20 cursor-pointer transition">
                    <div className="flex items-center gap-2 mb-1">
                      <MessageSquarePlus className="w-4 h-4 text-[#D4C5A9]" />
                      <span className="text-sm font-medium text-gray-800">Cuộc trò chuyện hiện tại</span>
                    </div>
                    <p className="text-xs text-gray-600 line-clamp-2">
                      {messages[0]?.content.substring(0, 60)}...
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      {messages.length} tin nhắn
                    </p>
                  </div>
                  <div className="text-xs text-gray-500 px-2 py-1">Các cuộc trò chuyện trước</div>
                  <div className="p-3 text-center text-sm text-gray-500">
                    Chưa có lịch sử trước đó
                  </div>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center h-full text-gray-400">
                  <History className="w-12 h-12 mb-2" />
                  <p className="text-sm">Chưa có lịch sử</p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Messages Container */}
        <div className="flex-1 overflow-y-auto px-4 py-4 bg-gray-50/50">
          <div className="max-w-3xl mx-auto space-y-3">
            {renderChatContent()}

            {isSending && (
              <div className="flex gap-2.5 justify-start">
                <div className="w-8 h-8 rounded-full bg-[#D4C5A9] flex items-center justify-center flex-shrink-0">
                  <Bot className="w-4 h-4 text-white" />
                </div>
                <div className="bg-white border border-gray-200 rounded-2xl px-4 py-2.5 shadow-sm">
                  <div className="flex gap-1">
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"></div>
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                    <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>
        </div>
      </div>

      {/* Input Area */}
      <div className="bg-white border-t border-gray-200 px-4 py-3 shadow-lg">
        <form onSubmit={handleSendMessage} className="max-w-3xl mx-auto">
          <div className="flex gap-2">
            <input
              type="text"
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              placeholder="Nhập câu hỏi của bạn..."
              className="flex-1 px-4 py-2.5 border border-gray-300 rounded-full focus:outline-none focus:ring-2 focus:ring-[#D4C5A9] focus:border-transparent text-sm"
              disabled={isSending}
            />
            <button
              type="submit"
              disabled={isSending || !inputMessage.trim()}
              className="w-10 h-10 bg-gradient-to-br from-[#D4C5A9] to-[#C4B599] text-white rounded-full flex items-center justify-center hover:shadow-md transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Send className="w-4 h-4" />
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ChatScreen;
