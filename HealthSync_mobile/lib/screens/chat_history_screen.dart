import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/chat_message.dart';
import '../services/chat_service.dart';
import 'chat_detail_screen.dart';

class ChatHistoryScreen extends StatefulWidget {
  const ChatHistoryScreen({super.key});

  @override
  State<ChatHistoryScreen> createState() => _ChatHistoryScreenState();
}

class ChatSession {
  final DateTime startTime;
  final DateTime endTime;
  final List<ChatMessage> messages;

  ChatSession({
    required this.startTime,
    required this.endTime,
    required this.messages,
  });

  String get previewText {
    // Find first user message for a good preview, or just the first message
    final firstMsg = messages.firstWhere((m) => m.isUser, orElse: () => messages.first);
    return firstMsg.content;
  }
}

class _ChatHistoryScreenState extends State<ChatHistoryScreen> {
  final ChatService _chatService = ChatService();
  bool _isLoading = true;
  List<ChatSession> _sessions = [];

  @override
  void initState() {
    super.initState();
    _loadHistory();
  }

  Future<void> _loadHistory() async {
    try {
      // Load more messages to build meaningful sessions
      final history = await _chatService.getChatHistory(pageSize: 100);
      
      // Sort: Oldest first to process, then we reverse sessions
      history.sort((a, b) => a.createdAt.compareTo(b.createdAt));

      final List<ChatSession> sessions = [];
      List<ChatMessage> currentBatch = [];
      
      if (history.isNotEmpty) {
        currentBatch.add(history.first);
        
        for (int i = 1; i < history.length; i++) {
          final prevMsg = history[i-1];
          final currentMsg = history[i];
          
          // If gap is more than 2 hours, start new session
          final difference = currentMsg.createdAt.difference(prevMsg.createdAt);
          if (difference.inHours >= 2) {
             sessions.add(ChatSession(
               startTime: currentBatch.first.createdAt,
               endTime: currentBatch.last.createdAt,
               messages: List.from(currentBatch),
             ));
             currentBatch = [];
          }
          currentBatch.add(currentMsg);
        }
        
        // Add last batch
        if (currentBatch.isNotEmpty) {
          sessions.add(ChatSession(
             startTime: currentBatch.first.createdAt,
             endTime: currentBatch.last.createdAt,
             messages: List.from(currentBatch),
          ));
        }
      }

      // Reverse so newest sessions are at top
      sessions.sort((a, b) => b.startTime.compareTo(a.startTime));

      if (mounted) {
        setState(() {
          _sessions = sessions;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi tải lịch sử: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFD9D7B6),
      appBar: AppBar(
        title: const Text(
          'Lịch sử trò chuyện',
          style: TextStyle(
            fontFamily: 'Estedad-VF',
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: const Color(0xFFFDFBD4),
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _sessions.isEmpty
              ? const Center(child: Text("Chưa có lịch sử trò chuyện"))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _sessions.length,
                  itemBuilder: (context, index) {
                    final session = _sessions[index];
                    return _buildSessionCard(session);
                  },
                ),
    );
  }

  Widget _buildSessionCard(ChatSession session) {
    final dateStr = _formatDate(session.startTime);
    final timeStr = DateFormat('HH:mm').format(session.startTime);
    
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      color: const Color(0xFFFDFBD4).withOpacity(0.8),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      elevation: 0,
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
               builder: (context) => ChatDetailScreen(
                 messages: session.messages,
                 title: '$dateStr $timeStr',
               ),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    dateStr,
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 14,
                      color: Color(0xFF556B2F),
                    ),
                  ),
                  Text(
                    timeStr,
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                session.previewText,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontSize: 14,
                  color: Colors.black87,
                  fontFamily: 'ABeeZee'
                ),
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  Icon(Icons.message_outlined, size: 14, color: Colors.grey[600]),
                  const SizedBox(width: 4),
                  Text(
                    '${session.messages.length} tin nhắn',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    if (date.year == now.year && date.month == now.month && date.day == now.day) {
      return "Hôm nay";
    } else if (date.year == now.year && date.month == now.month && date.day == now.day - 1) {
      return "Hôm qua";
    } else {
      return DateFormat('dd/MM/yyyy').format(date);
    }
  }
}

