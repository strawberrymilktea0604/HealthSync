class ChatMessage {
  final String id;
  final String role;
  final String content;
  final DateTime createdAt;

  ChatMessage({
    required this.id,
    required this.role,
    required this.content,
    required this.createdAt,
  });

  factory ChatMessage.fromJson(Map<String, dynamic> json) {
    return ChatMessage(
      id: json['messageId'] ?? '',
      role: json['role'] ?? 'user',
      content: json['content'] ?? '',
      createdAt: DateTime.parse(json['createdAt']),
    );
  }

  bool get isUser => role == 'user';
}

class ChatResponse {
  final String response;
  final DateTime timestamp;
  final String messageId;

  ChatResponse({
    required this.response,
    required this.timestamp,
    required this.messageId,
  });

  factory ChatResponse.fromJson(Map<String, dynamic> json) {
    return ChatResponse(
      response: json['response'] ?? '',
      timestamp: DateTime.parse(json['timestamp']),
      messageId: json['messageId'] ?? '',
    );
  }
}
