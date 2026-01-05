import 'package:flutter/material.dart';
import 'package:flutter_markdown/flutter_markdown.dart';
import '../models/chat_message.dart';
import 'package:intl/intl.dart';

class ChatDetailScreen extends StatelessWidget {
  final List<ChatMessage> messages;
  final String title;

  const ChatDetailScreen({
    super.key, 
    required this.messages,
    required this.title,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFEBE8D0),
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Chi tiết cuộc trò chuyện',
              style: TextStyle(
                fontFamily: 'Estedad-VF',
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.black,
              ),
            ),
            Text(
              title,
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey[700],
                fontWeight: FontWeight.w400,
              ),
            ),
          ],
        ),
        backgroundColor: const Color(0xFFFDFBD4),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: ListView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
        itemCount: messages.length,
        itemBuilder: (context, index) {
          final message = messages[index];
          return _ChatBubble(message: message);
        },
      ),
    );
  }
}

class _ChatBubble extends StatelessWidget {
  final ChatMessage message;

  const _ChatBubble({required this.message});

  @override
  Widget build(BuildContext context) {
    final isUser = message.isUser;
    
    return Padding(
      padding: const EdgeInsets.only(bottom: 24),
      child: Row(
        mainAxisAlignment:
            isUser ? MainAxisAlignment.end : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (!isUser) ...[
            const CircleAvatar(
              backgroundColor: Color(0xFFD4C5A9),
              backgroundImage: AssetImage('assets/images/logo.png'),
              radius: 16,
              child: Icon(Icons.smart_toy, size: 20, color: Colors.white),
            ),
            const SizedBox(width: 8),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: isUser
                    ? const Color(0xFFAA55EE) 
                    : const Color(0xFFD4C5A9),
                borderRadius: BorderRadius.only(
                  topLeft: isUser ? const Radius.circular(20) : Radius.zero,
                  topRight: isUser ? const Radius.circular(20) : const Radius.circular(20),
                  bottomLeft: const Radius.circular(20),
                  bottomRight: isUser ? Radius.zero : const Radius.circular(20),
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 5,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  MarkdownBody(
                    data: message.content,
                    selectable: true,
                    styleSheet: MarkdownStyleSheet(
                      p: TextStyle(
                        fontFamily: 'ABeeZee',
                        fontSize: 15,
                        height: 1.4,
                        color: isUser ? Colors.white : Colors.black87,
                      ),
                      code: TextStyle(
                        backgroundColor: isUser ? Colors.white24 : Colors.black12,
                        fontFamily: 'monospace',
                        color: isUser ? Colors.white : Colors.black87,
                      ),
                      codeblockDecoration: BoxDecoration(
                        color: isUser ? Colors.white24 : Colors.black12,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      tableHead: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 11,
                        color: isUser ? Colors.white : Colors.black87,
                      ),
                      tableBody: TextStyle(
                        fontSize: 11,
                        color: isUser ? Colors.white : Colors.black87,
                      ),
                      tableCellsPadding: const EdgeInsets.all(4),
                      tableBorder: TableBorder.all(
                        color: isUser ? Colors.white54 : Colors.black26, 
                        width: 0.5
                      ),
                    ),
                  ),
                  const SizedBox(height: 4),
                  Align(
                    alignment: Alignment.bottomRight,
                    child: Text(
                      DateFormat('HH:mm').format(message.createdAt),
                      style: TextStyle(
                        fontSize: 10,
                        color: isUser ? Colors.white70 : Colors.black54,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (isUser) ...[
            const SizedBox(width: 8),
            const CircleAvatar(
              backgroundColor: Colors.transparent,
              radius: 16,
              child: Icon(Icons.person_outline, size: 24, color: Colors.black54),
            ),
          ],
        ],
      ),
    );
  }
}
