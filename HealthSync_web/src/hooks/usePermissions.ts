import { useAuth } from '@/contexts/AuthContext';
import { Permission, ROLE_PERMISSIONS, Role, PermissionCheck } from '@/types/rbac';

export const usePermissions = () => {
  const { user } = useAuth();

  /**
   * Check if user has a specific permission
   */
  const hasPermission = (permission: Permission): boolean => {
    if (!user) return false;
    
    const userRole = user.role as Role;
    const rolePermissions = ROLE_PERMISSIONS[userRole];
    
    return rolePermissions?.includes(permission) || false;
  };

  /**
   * Check if user has all specified permissions
   */
  const hasAllPermissions = (permissions: Permission[]): boolean => {
    return permissions.every(permission => hasPermission(permission));
  };

  /**
   * Check if user has any of the specified permissions
   */
  const hasAnyPermission = (permissions: Permission[]): boolean => {
    return permissions.some(permission => hasPermission(permission));
  };

  /**
   * Check if user has permission with detailed result
   */
  const checkPermission = (permission: Permission): PermissionCheck => {
    if (!user) {
      return {
        hasPermission: false,
        reason: 'User not authenticated'
      };
    }

    const hasAccess = hasPermission(permission);
    
    return {
      hasPermission: hasAccess,
      reason: hasAccess ? undefined : 'Insufficient permissions'
    };
  };

  /**
   * Check if user is admin
   */
  const isAdmin = (): boolean => {
    return user?.role === Role.ADMIN;
  };

  /**
   * Check if user is regular user
   */
  const isUser = (): boolean => {
    return user?.role === Role.USER;
  };

  /**
   * Get all permissions for current user
   */
  const getUserPermissions = (): Permission[] => {
    if (!user) return [];
    
    const userRole = user.role as Role;
    return ROLE_PERMISSIONS[userRole] || [];
  };

  return {
    hasPermission,
    hasAllPermissions,
    hasAnyPermission,
    checkPermission,
    isAdmin,
    isUser,
    getUserPermissions,
    userRole: user?.role,
  };
};
