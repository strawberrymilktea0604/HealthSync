import { useState, useEffect, useRef } from "react";
import { chatService } from "@/services/chatService";
import { ChatMessage } from "@/types/chat";

export function useChat(showChat: boolean) {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [inputMessage, setInputMessage] = useState("");
    const [isSending, setIsSending] = useState(false);
    const [loadingChat, setLoadingChat] = useState(false);
    const [showChatHistory, setShowChatHistory] = useState(false);
    const [historyList, setHistoryList] = useState<ChatMessage[]>([]);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (showChat) {
            const userJson = localStorage.getItem('user');
            if (userJson) {
                loadChatHistory();
            } else {
                console.warn('User not logged in - cannot load chat history');
            }
        }
    }, [showChat]);

    useEffect(() => {
        if (showChatHistory) {
            loadSidebarHistory();
        }
    }, [showChatHistory]);

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    const loadSidebarHistory = async () => {
        try {
            const history = await chatService.getChatHistory();
            setHistoryList(history);
        } catch (error: any) {
            console.error('Error loading sidebar history:', error);
        }
    };

    const loadChatHistory = async () => {
        setLoadingChat(true);
        try {
            const history = await chatService.getChatHistory();
            setMessages(history);
            setHistoryList(history);
        } catch (error: any) {
            console.error('Error loading chat history:', error);
            if (error.response?.status === 401) {
                console.warn('User not authenticated for chat');
            }
            setMessages([]);
        } finally {
            setLoadingChat(false);
        }
    };

    const handleNewChat = () => {
        if (messages.length > 0) {
            const confirmNew = globalThis.confirm('Bạn có muốn bắt đầu cuộc trò chuyện mới? Cuộc trò chuyện hiện tại sẽ được lưu vào lịch sử.');
            if (!confirmNew) return;
        }
        setMessages([]);
        setInputMessage('');
        setShowChatHistory(false);
    };

    const handleSendMessage = async (e: React.FormEvent) => {
        e.preventDefault();

        const question = inputMessage.trim();
        if (!question || isSending) return;

        const userMessage: ChatMessage = {
            id: Date.now().toString(),
            role: 'user',
            content: question,
            createdAt: new Date().toISOString(),
        };

        setMessages((prev) => [...prev, userMessage]);
        setHistoryList((prev) => [...prev, userMessage]);

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
            setHistoryList((prev) => [...prev, aiMessage]);
        } catch (error: any) {
            console.error('Error sending message:', error);

            const errorMessage: ChatMessage = {
                id: (Date.now() + 1).toString(),
                role: 'assistant',
                content: 'Xin lỗi, tôi không thể trả lời câu hỏi của bạn lúc này. Vui lòng thử lại sau.',
                createdAt: new Date().toISOString(),
            };
            setMessages((prev) => [...prev, errorMessage]);
        } finally {
            setIsSending(false);
        }
    };

    return {
        messages,
        inputMessage,
        setInputMessage,
        isSending,
        loadingChat,
        showChatHistory,
        setShowChatHistory,
        historyList,
        messagesEndRef,
        loadSidebarHistory,
        handleNewChat,
        handleSendMessage
    };
}
