import { Permission } from '@/types/rbac';
import { PermissionGuard } from './PermissionGuard';

/**
 * Higher-order component for permission-based component wrapping
 * 
 * Usage:
 * const ProtectedComponent = withPermission(
 *   MyComponent, 
 *   Permission.VIEW_USERS
 * );
 */
export const withPermission = <P extends object>(
  Component: React.ComponentType<P>,
  permission: Permission | Permission[],
  requireAll = true
) => {
  const WrappedComponent = (props: P) => {
    const permissions = Array.isArray(permission) ? permission : [permission];
    
    return (
      <PermissionGuard permissions={permissions} requireAll={requireAll}>
        <Component {...props} />
      </PermissionGuard>
    );
  };

  // Set display name for better debugging
  WrappedComponent.displayName = `withPermission(${Component.displayName || Component.name || 'Component'})`;

  return WrappedComponent;
};
