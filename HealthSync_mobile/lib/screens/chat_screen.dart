import 'package:flutter/material.dart';
import '../models/chat_message.dart';
import '../services/chat_service.dart';

class ChatScreen extends StatefulWidget {
  const ChatScreen({Key? key}) : super(key: key);

  @override
  State<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends State<ChatScreen> {
  final TextEditingController _messageController = TextEditingController();
  final ScrollController _scrollController = ScrollController();
  final ChatService _chatService = ChatService();
  
  List<ChatMessage> _messages = [];
  bool _isLoading = false;
  bool _isSending = false;

  @override
  void initState() {
    super.initState();
    _loadChatHistory();
  }

  Future<void> _loadChatHistory() async {
    setState(() => _isLoading = true);
    try {
      final history = await _chatService.getChatHistory();
      setState(() {
        _messages = history;
        _isLoading = false;
      });
      _scrollToBottom();
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải lịch sử: ${e.toString()}')),
        );
      }
    }
  }

  Future<void> _sendMessage() async {
    final question = _messageController.text.trim();
    if (question.isEmpty || _isSending) return;

    // Add user message immediately
    final userMessage = ChatMessage(
      id: DateTime.now().toString(),
      role: 'user',
      content: question,
      createdAt: DateTime.now(),
    );

    setState(() {
      _messages.add(userMessage);
      _isSending = true;
    });
    _messageController.clear();
    _scrollToBottom();

    try {
      final response = await _chatService.sendMessage(question);
      
      final aiMessage = ChatMessage(
        id: response.messageId,
        role: 'assistant',
        content: response.response,
        createdAt: response.timestamp,
      );

      setState(() {
        _messages.add(aiMessage);
        _isSending = false;
      });
      _scrollToBottom();
    } catch (e) {
      setState(() => _isSending = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString()),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 300),
          curve: Curves.easeOut,
        );
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        title: const Text(
          'HealthBot 💪',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontSize: 20,
            fontWeight: FontWeight.w700,
          ),
        ),
        backgroundColor: const Color(0xFFFDFBD4),
        elevation: 4,
        shadowColor: Colors.black.withValues(alpha: 0.25),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadChatHistory,
          ),
        ],
      ),
      body: Column(
        children: [
          // Messages List
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : _messages.isEmpty
                    ? Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.chat_bubble_outline,
                              size: 80,
                              color: Colors.grey[300],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              'Bắt đầu cuộc trò chuyện',
                              style: TextStyle(
                                fontSize: 18,
                                color: Colors.grey[600],
                              ),
                            ),
                          ],
                        ),
                      )
                    : ListView.builder(
                        controller: _scrollController,
                        padding: const EdgeInsets.all(16),
                        itemCount: _messages.length,
                        itemBuilder: (context, index) {
                          final message = _messages[index];
                          return _ChatBubble(message: message);
                        },
                      ),
          ),

          // Loading indicator
          if (_isSending)
            Container(
              padding: const EdgeInsets.all(8),
              child: Row(
                children: [
                  const SizedBox(width: 16),
                  SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      valueColor: AlwaysStoppedAnimation<Color>(
                        Colors.grey[400]!,
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'AI đang suy nghĩ...',
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),

          // Input Area
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
            decoration: BoxDecoration(
              color: const Color(0xFFFDFBD4),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.25),
                  blurRadius: 11.4,
                  offset: const Offset(0, -13),
                ),
              ],
            ),
            child: SafeArea(
              child: Row(
                children: [
                  Expanded(
                    child: Container(
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(24),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withValues(alpha: 0.25),
                            blurRadius: 5.6,
                            offset: const Offset(0, 6),
                          ),
                        ],
                      ),
                      child: TextField(
                        controller: _messageController,
                        decoration: const InputDecoration(
                          hintText: 'Type a Message',
                          hintStyle: TextStyle(
                            fontFamily: 'ABeeZee',
                            fontSize: 24,
                            fontWeight: FontWeight.w400,
                            color: Color(0xFFB6B7B8),
                            letterSpacing: 0.0073,
                          ),
                          border: InputBorder.none,
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 8,
                          ),
                        ),
                        maxLines: null,
                        textCapitalization: TextCapitalization.sentences,
                        enabled: !_isSending,
                        onSubmitted: (_) => _sendMessage(),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  IconButton(
                    icon: const Icon(Icons.send, color: Colors.black),
                    onPressed: _isSending ? null : _sendMessage,
                    iconSize: 24,
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }
}

class _ChatBubble extends StatelessWidget {
  final ChatMessage message;

  const _ChatBubble({required this.message});

  @override
  Widget build(BuildContext context) {
    final isUser = message.isUser;
    
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        mainAxisAlignment:
            isUser ? MainAxisAlignment.end : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (!isUser) ...[
            CircleAvatar(
              backgroundColor: const Color(0xFFD4C5A9),
              radius: 16,
              child: const Text('🤖', style: TextStyle(fontSize: 16)),
            ),
            const SizedBox(width: 8),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 18.85, vertical: 18.64),
              decoration: BoxDecoration(
                color: isUser
                    ? const Color(0xFFAA55EE)
                    : const Color(0xFFCCC997),
                borderRadius: BorderRadius.only(
                  topLeft: isUser ? const Radius.circular(24) : Radius.zero,
                  topRight: isUser ? const Radius.circular(24) : const Radius.circular(24),
                  bottomLeft: const Radius.circular(24),
                  bottomRight: isUser ? Radius.zero : const Radius.circular(24),
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    message.content,
                    style: TextStyle(
                      fontFamily: 'ABeeZee',
                      fontSize: 14,
                      fontWeight: FontWeight.w400,
                      color: isUser ? Colors.white : const Color(0xFF77838F),
                      letterSpacing: 0.0714,
                      height: 1.714,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    _formatTime(message.createdAt),
                    style: TextStyle(
                      fontFamily: 'ABeeZee',
                      fontSize: 10,
                      color: isUser
                          ? Colors.white.withValues(alpha: 0.7)
                          : Colors.black54,
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (isUser) ...[
            const SizedBox(width: 8),
            CircleAvatar(
              backgroundColor: Colors.blue,
              radius: 16,
              child: const Icon(Icons.person, size: 18, color: Colors.white),
            ),
          ],
        ],
      ),
    );
  }

  String _formatTime(DateTime dateTime) {
    return '${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';
  }
}
