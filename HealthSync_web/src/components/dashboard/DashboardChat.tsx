import { Bot, MessageSquarePlus, History, RefreshCw, Loader2, Send, X, User } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { useChat } from "./hooks/useChat";
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

interface DashboardChatProps {
    showChat: boolean;
    setShowChat: (show: boolean) => void;
}

export default function DashboardChat({ showChat, setShowChat }: DashboardChatProps) {
    const {
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
    } = useChat(showChat);

    const formatTime = (dateString: string) => {
        if (!dateString) return "";
        const normalizedDateString = !dateString.endsWith("Z") && !dateString.includes("+")
            ? dateString + "Z"
            : dateString;
        const date = new Date(normalizedDateString);
        return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    };

    const renderChatContent = () => {
        if (loadingChat) {
            return (
                <div className="flex justify-center items-center h-full">
                    <div className="p-4 bg-white rounded-2xl shadow-sm border border-gray-100">
                        <Loader2 className="w-8 h-8 animate-spin text-[#2d2d2d]" />
                    </div>
                </div>
            );
        }

        if (messages.length === 0) {
            return (
                <div className="flex flex-col items-center justify-center h-full text-gray-400 animate-in fade-in zoom-in duration-500">
                    <div className="w-24 h-24 rounded-[2rem] bg-gradient-to-br from-[#EBE9C0] to-[#E5E3B5] shadow-xl flex items-center justify-center mb-6 group transform transition-transform hover:scale-105 duration-300">
                        <Bot className="w-12 h-12 text-[#2d2d2d] group-hover:rotate-12 transition-transform duration-300" />
                    </div>
                    <h3 className="text-xl font-bold text-[#2d2d2d] mb-2">Xin chào! 👋</h3>
                    <p className="text-sm text-gray-500 text-center max-w-[200px] leading-relaxed">
                        Tôi là trợ lý sức khỏe AI của bạn. Hãy hỏi tôi bất cứ điều gì!
                    </p>
                </div>
            );
        }

        return messages.map((message) => (
            <motion.div
                key={message.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.3 }}
                className={`flex gap-4 items-end ${message.role === 'user' ? 'flex-row-reverse' : 'flex-row'}`}
            >
                {/* Avatar Block */}
                <div className={`w-[70px] h-[60px] rounded-[1.25rem] flex items-center justify-center flex-shrink-0 shadow-sm border-2 ${message.role === 'assistant'
                    ? 'bg-[#EBE9C0] border-white'
                    : 'bg-[#2563EB] border-blue-400'
                    }`}>
                    {message.role === 'assistant' ? (
                        <Bot className="w-8 h-8 text-[#2d2d2d]" />
                    ) : (
                        <User className="w-8 h-8 text-white" />
                    )}
                </div>

                {/* Content Bubble */}
                <div
                    className={`max-w-[70%] rounded-[1.5rem] px-5 py-4 shadow-md text-[15px] leading-relaxed relative ${message.role === 'user'
                        ? 'bg-[#1a1a1a] text-white rounded-tr-md'
                        : 'bg-white border border-gray-100 text-gray-800 rounded-tl-md'
                        }`}
                >
                    {message.role === 'user' ? (
                        <p className="whitespace-pre-wrap font-medium">{message.content}</p>
                    ) : (
                        <div className="prose prose-neutral max-w-none 
                            prose-headings:text-gray-900 prose-headings:font-bold prose-headings:mb-2 prose-headings:mt-4
                            prose-p:text-gray-800 prose-p:leading-7 
                            prose-strong:text-black prose-strong:font-black
                            prose-ul:list-disc prose-ul:pl-4 prose-ul:my-2
                            prose-li:my-1
                            prose-code:bg-gray-100 prose-code:px-1.5 prose-code:py-0.5 prose-code:rounded-md prose-code:text-sm prose-code:font-mono prose-code:text-pink-600
                            first:mt-0 last:mb-0"
                        >
                            <ReactMarkdown remarkPlugins={[remarkGfm]}>
                                {message.content}
                            </ReactMarkdown>
                        </div>
                    )}
                    <p
                        className={`text-[10px] mt-2 font-medium ${message.role === 'user' ? 'text-white/40 text-left' : 'text-gray-400 text-right'
                            }`}
                    >
                        {formatTime(message.createdAt)}
                    </p>
                </div>
            </motion.div>
        ));
    };

    return (
        <div className="fixed bottom-8 right-8 z-50">
            <AnimatePresence>
                {showChat && (
                    <motion.div
                        initial={{ opacity: 0, scale: 0.9, y: 20 }}
                        animate={{ opacity: 1, scale: 1, y: 0 }}
                        exit={{ opacity: 0, scale: 0.9, y: 20 }}
                        transition={{ duration: 0.2 }}
                        className="absolute bottom-20 right-0 w-[420px] h-[650px] bg-white rounded-3xl shadow-2xl overflow-hidden border border-gray-100 flex flex-col font-sans"
                    >
                        {/* Header */}
                        <div className="bg-[#EBE9C0] px-6 py-4 flex justify-between items-center shadow-sm sticky top-0 z-10">
                            <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-[#2d2d2d] flex items-center justify-center shadow-sm">
                                    <Bot className="w-6 h-6 text-[#EBE9C0]" />
                                </div>
                                <div>
                                    <h3 className="font-bold text-[#2d2d2d] text-lg">Assistant</h3>
                                    <p className="text-xs text-[#2d2d2d]/60 font-medium flex items-center gap-1.5">
                                        <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse shadow-[0_0_8px_rgba(34,197,94,0.6)]"></span>
                                        Online
                                    </p>
                                </div>
                            </div>
                            <div className="flex items-center gap-1">
                                <button
                                    onClick={handleNewChat}
                                    className="p-2.5 hover:bg-black/5 rounded-xl transition-colors text-[#2d2d2d]"
                                    title="Mới"
                                >
                                    <MessageSquarePlus className="w-7 h-7" />
                                </button>
                                <button
                                    onClick={() => setShowChatHistory(!showChatHistory)}
                                    className={`p-2.5 hover:bg-black/5 rounded-xl transition-colors text-[#2d2d2d] ${showChatHistory ? 'bg-black/10' : ''}`}
                                    title="Lịch sử"
                                >
                                    <History className="w-7 h-7" />
                                </button>
                                <button
                                    onClick={() => setShowChat(false)}
                                    className="p-2.5 hover:bg-red-500/10 hover:text-red-600 rounded-xl transition-colors text-[#2d2d2d]/60 ml-1"
                                >
                                    <X className="w-8 h-8" />
                                </button>
                            </div>
                        </div>

                        {/* Main Content Area */}
                        <div className="flex-1 flex overflow-hidden bg-gray-50/30 relative">

                            {/* Background Pattern */}
                            <div className="absolute inset-0 opacity-[0.03] pointer-events-none"
                                style={{ backgroundImage: `radial-gradient(#2d2d2d 1px, transparent 1px)`, backgroundSize: '24px 24px' }}>
                            </div>

                            {/* Chat History Sidebar */}
                            {showChatHistory && (
                                <motion.div
                                    initial={{ width: 0, opacity: 0 }}
                                    animate={{ width: 240, opacity: 1 }}
                                    exit={{ width: 0, opacity: 0 }}
                                    className="bg-white/95 backdrop-blur-md border-r border-gray-100 flex flex-col h-full overflow-hidden z-20 shadow-lg"
                                >
                                    <div className="p-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
                                        <span className="text-xs font-bold text-gray-400 uppercase tracking-widest">Lịch sử</span>
                                        <button onClick={loadSidebarHistory} title="Làm mới" className="p-1 hover:bg-gray-200 rounded-full text-gray-400 hover:text-gray-600 transition-colors">
                                            <RefreshCw className="w-3 h-3" />
                                        </button>
                                    </div>
                                    <div className="flex-1 overflow-y-auto p-3 space-y-2">
                                        {historyList.length > 0 ? (
                                            <>
                                                <div className="mb-2 px-2">
                                                    <span className="text-[10px] font-bold text-gray-400 uppercase">Gần đây</span>
                                                </div>
                                                {historyList.filter(m => m.role === 'user').slice().reverse().map((msg, idx) => (
                                                    <div key={msg.id || idx} className="p-3 bg-gray-50 hover:bg-[#EBE9C0]/30 rounded-xl border border-gray-100 hover:border-[#Dcdbb0] shadow-sm cursor-pointer transition-colors group">
                                                        <p className="text-xs text-[#2d2d2d] font-medium line-clamp-2 leading-relaxed group-hover:text-black">
                                                            {msg.content}
                                                        </p>
                                                        <p className="text-[10px] text-gray-400 mt-1.5 text-right">{formatTime(msg.createdAt)}</p>
                                                    </div>
                                                ))}
                                            </>
                                        ) : (
                                            <div className="flex flex-col items-center justify-center h-40 text-gray-400">
                                                {loadingChat ? (
                                                    <Loader2 className="w-5 h-5 animate-spin mb-2" />
                                                ) : (
                                                    <>
                                                        <History className="w-8 h-8 text-gray-200 mb-2" />
                                                        <p className="text-xs font-medium">Trống</p>
                                                    </>
                                                )}
                                            </div>
                                        )}
                                    </div>
                                </motion.div>
                            )}

                            {/* Chat Messages */}
                            <div className="flex-1 overflow-y-auto p-5 space-y-6 scrollbar-thin scrollbar-thumb-gray-200 scrollbar-track-transparent z-10">
                                {renderChatContent()}
                                {isSending && (
                                    <motion.div
                                        initial={{ opacity: 0, y: 10 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        className="flex gap-4 items-end"
                                    >
                                        <div className="w-[70px] h-[60px] rounded-[1.25rem] flex items-center justify-center flex-shrink-0 shadow-sm border-2 bg-[#EBE9C0] border-white">
                                            <Bot className="w-8 h-8 text-[#2d2d2d]" />
                                        </div>
                                        <div className="bg-white border border-gray-100 rounded-[1.5rem] rounded-tl-md px-6 py-5 shadow-sm">
                                            <div className="flex gap-2 items-center h-4">
                                                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.3s]"></div>
                                                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce [animation-delay:-0.15s]"></div>
                                                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"></div>
                                            </div>
                                        </div>
                                    </motion.div>
                                )}
                                <div ref={messagesEndRef} />
                            </div>
                        </div>

                        {/* Input Form */}
                        <div className="p-4 bg-white border-t border-gray-100">
                            <form onSubmit={handleSendMessage} className="flex items-center gap-3">
                                <input
                                    type="text"
                                    value={inputMessage}
                                    onChange={(e) => setInputMessage(e.target.value)}
                                    placeholder="Nhập câu hỏi của bạn..."
                                    disabled={isSending}
                                    className="flex-1 bg-gray-50 text-gray-800 placeholder-gray-400 border-2 border-gray-100 rounded-full py-4 pl-6 pr-6 focus:ring-0 focus:border-[#2d2d2d] transition-all text-base font-medium shadow-inner h-14"
                                />
                                <motion.button
                                    whileHover={{ scale: 1.05 }}
                                    whileTap={{ scale: 0.95 }}
                                    type="submit"
                                    disabled={isSending || !inputMessage.trim()}
                                    className="w-20 h-14 bg-[#2d2d2d] text-[#FDFBD4] rounded-full flex items-center justify-center hover:bg-black transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl"
                                >
                                    {isSending ? <Loader2 className="w-6 h-6 animate-spin" /> : <Send className="w-6 h-6" />}
                                </motion.button>
                            </form>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
            <motion.button
                whileHover={{ scale: 1.08, rotate: 5 }}
                whileTap={{ scale: 0.92 }}
                onClick={() => setShowChat(!showChat)}
                className="w-16 h-16 bg-gradient-to-br from-[#2d2d2d] to-[#1a1a1a] rounded-2xl shadow-2xl flex items-center justify-center text-[#EBE9C0] hover:shadow-[#2d2d2d]/30 transition-all ring-2 ring-white/10"
            >
                <Bot className="w-8 h-8" />
            </motion.button>
        </div>
    );
}
