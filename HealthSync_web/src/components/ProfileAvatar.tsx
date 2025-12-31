import React, { useState, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Camera, Upload } from 'lucide-react';
import { toast } from '@/hooks/use-toast';
import { useAuth } from '@/contexts/AuthContext';
import api from '@/services/api';

const ProfileAvatar: React.FC = () => {
  const { user, setUser } = useAuth();
  const [avatarUrl, setAvatarUrl] = useState(user?.avatarUrl || '');
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
      toast({
        title: 'Lỗi',
        description: 'Chỉ chấp nhận file ảnh (JPEG, PNG, GIF)',
        variant: 'destructive',
      });
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast({
        title: 'Lỗi',
        description: 'Kích thước file phải nhỏ hơn 5MB',
        variant: 'destructive',
      });
      return;
    }

    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await api.post('/userprofile/upload-avatar', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      const newAvatarUrl = response.data.avatarUrl;
      setAvatarUrl(newAvatarUrl);

      // Update user context
      if (user) {
        setUser({ ...user, avatarUrl: newAvatarUrl });
      }

      toast({
        title: 'Thành công',
        description: 'Đã cập nhật ảnh đại diện',
      });
    } catch (error: unknown) {
      console.error('Error uploading avatar:', error);
      const errorMessage = error && typeof error === 'object' && 'response' in error
        ? (error as { response?: { data?: { message?: string } } }).response?.data?.message
        : 'Không thể tải ảnh lên';
      toast({
        title: 'Lỗi',
        description: errorMessage || 'Không thể tải ảnh lên',
        variant: 'destructive',
      });
    } finally {
      setIsUploading(false);
    }
  };

  const triggerFileInput = () => {
    fileInputRef.current?.click();
  };

  const getInitials = () => {
    if (!user?.fullName) return 'U';
    const names = user.fullName.split(' ');
    return names.length > 1
      ? `${names[0][0]}${names.at(-1)?.[0]}`
      : names[0][0];
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ảnh đại diện</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col items-center space-y-4">
        <div className="relative">
          <Avatar className="h-32 w-32">
            <AvatarImage src={avatarUrl ? `${avatarUrl}?t=${Date.now()}` : avatarUrl} alt={user?.fullName || 'User'} />
            <AvatarFallback className="text-3xl">
              {getInitials()}
            </AvatarFallback>
          </Avatar>
          <Button
            size="sm"
            className="absolute bottom-0 right-0 rounded-full h-10 w-10 p-0"
            onClick={triggerFileInput}
            disabled={isUploading}
          >
            <Camera className="h-4 w-4" />
          </Button>
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          onChange={handleFileSelect}
          className="hidden"
        />

        <Button
          variant="outline"
          onClick={triggerFileInput}
          disabled={isUploading}
          className="w-full"
        >
          <Upload className="mr-2 h-4 w-4" />
          {isUploading ? 'Đang tải lên...' : 'Tải ảnh lên'}
        </Button>

        <p className="text-xs text-gray-500 text-center">
          Chấp nhận: JPG, PNG, GIF (tối đa 5MB)
        </p>
      </CardContent>
    </Card>
  );
};

export default ProfileAvatar;
