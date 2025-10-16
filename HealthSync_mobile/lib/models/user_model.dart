class User {
  final int userId;
  final String email;
  final String fullName;
  final String role;
  final String token;
  final DateTime expiresAt;

  User({
    required this.userId,
    required this.email,
    required this.fullName,
    required this.role,
    required this.token,
    required this.expiresAt,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      userId: json['userId'] ?? json['UserId'],
      email: json['email'] ?? json['Email'],
      fullName: json['fullName'] ?? json['FullName'],
      role: json['role'] ?? json['Role'],
      token: json['token'] ?? json['Token'],
      expiresAt: DateTime.parse(json['expiresAt'] ?? json['ExpiresAt']),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'email': email,
      'fullName': fullName,
      'role': role,
      'token': token,
      'expiresAt': expiresAt.toIso8601String(),
    };
  }
}
