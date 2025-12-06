import 'dart:io';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../services/api_service.dart';
import 'package:http/http.dart' as http;

class ProfileAvatarWidget extends StatefulWidget {
  const ProfileAvatarWidget({super.key});

  @override
  State<ProfileAvatarWidget> createState() => _ProfileAvatarWidgetState();
}

class _ProfileAvatarWidgetState extends State<ProfileAvatarWidget> {
  final ImagePicker _picker = ImagePicker();
  bool _isUploading = false;

  Future<void> _pickAndUploadImage(ImageSource source) async {
    try {
      final XFile? image = await _picker.pickImage(
        source: source,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 85,
      );

      if (image == null) return;

      // Check file size (max 5MB)
      final file = File(image.path);
      final fileSize = await file.length();
      if (fileSize > 5 * 1024 * 1024) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Kích thước file phải nhỏ hơn 5MB')),
          );
        }
        return;
      }

      setState(() => _isUploading = true);

      if (!mounted) return;
      
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final token = authProvider.user?.token;

      if (token == null) {
        throw Exception('Chưa đăng nhập');
      }

      // Create multipart request
      final request = http.MultipartRequest(
        'POST',
        Uri.parse('${ApiService.baseUrl}/userprofile/upload-avatar'),
      );

      request.headers['Authorization'] = 'Bearer $token';
      request.files.add(await http.MultipartFile.fromPath('file', image.path));

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);

      if (response.statusCode == 200) {
        final jsonResponse = jsonDecode(response.body);
        final avatarUrl = jsonResponse['avatarUrl'];

        // Update user with new avatar URL
        authProvider.updateUserAvatar(avatarUrl);

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Đã cập nhật ảnh đại diện')),
          );
        }
      } else {
        throw Exception('Không thể tải ảnh lên');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: ${e.toString()}')),
        );
      }
    } finally {
      setState(() => _isUploading = false);
    }
  }

  void _showImageSourceDialog() {
    showModalBottomSheet(
      context: context,
      builder: (context) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: Icon(Icons.photo_camera),
              title: Text('Chụp ảnh'),
              onTap: () {
                Navigator.pop(context);
                _pickAndUploadImage(ImageSource.camera);
              },
            ),
            ListTile(
              leading: Icon(Icons.photo_library),
              title: Text('Chọn từ thư viện'),
              onTap: () {
                Navigator.pop(context);
                _pickAndUploadImage(ImageSource.gallery);
              },
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final user = authProvider.user;
    final avatarUrl = user?.avatarUrl;

    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: EdgeInsets.all(20),
        child: Column(
          children: [
            Text(
              'Ảnh đại diện',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            SizedBox(height: 20),
            Stack(
              children: [
                CircleAvatar(
                  radius: 60,
                  backgroundColor: Color(0xFFB8C5A0),
                  backgroundImage: avatarUrl != null && avatarUrl.isNotEmpty
                      ? NetworkImage(avatarUrl)
                      : null,
                  child: avatarUrl == null || avatarUrl.isEmpty
                      ? Text(
                          _getInitials(user?.fullName ?? 'U'),
                          style: TextStyle(
                            fontSize: 36,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                        )
                      : null,
                ),
                if (_isUploading)
                  Positioned.fill(
                    child: CircleAvatar(
                      radius: 60,
                      backgroundColor: Colors.black54,
                      child: CircularProgressIndicator(
                        color: Colors.white,
                      ),
                    ),
                  ),
                Positioned(
                  bottom: 0,
                  right: 0,
                  child: Container(
                    decoration: BoxDecoration(
                      color: Color(0xFFB8C5A0),
                      shape: BoxShape.circle,
                      border: Border.all(color: Colors.white, width: 2),
                    ),
                    child: IconButton(
                      icon: Icon(Icons.camera_alt, color: Colors.white, size: 20),
                      onPressed: _isUploading ? null : _showImageSourceDialog,
                    ),
                  ),
                ),
              ],
            ),
            SizedBox(height: 20),
            ElevatedButton.icon(
              onPressed: _isUploading ? null : _showImageSourceDialog,
              icon: Icon(Icons.upload),
              label: Text(_isUploading ? 'Đang tải lên...' : 'Tải ảnh lên'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Color(0xFFB8C5A0),
                minimumSize: Size(double.infinity, 45),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
            SizedBox(height: 8),
            Text(
              'JPG, PNG, GIF (tối đa 5MB)',
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _getInitials(String name) {
    if (name.isEmpty) return 'U';
    final names = name.trim().split(' ');
    if (names.length > 1) {
      return '${names[0][0]}${names[names.length - 1][0]}'.toUpperCase();
    }
    return name[0].toUpperCase();
  }
}
