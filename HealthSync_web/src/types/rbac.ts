// RBAC Types - Role-Based Access Control
export enum Role {
  ADMIN = 'Admin',
  USER = 'User'
}

export enum Permission {
  // User Management
  VIEW_USERS = 'view_users',
  CREATE_USERS = 'create_users',
  UPDATE_USERS = 'update_users',
  DELETE_USERS = 'delete_users',
  UPDATE_USER_ROLES = 'update_user_roles',
  
  // Content Library - Exercises
  VIEW_EXERCISES = 'view_exercises',
  CREATE_EXERCISES = 'create_exercises',
  UPDATE_EXERCISES = 'update_exercises',
  DELETE_EXERCISES = 'delete_exercises',
  
  // Content Library - Foods
  VIEW_FOODS = 'view_foods',
  CREATE_FOODS = 'create_foods',
  UPDATE_FOODS = 'update_foods',
  DELETE_FOODS = 'delete_foods',
  
  // Dashboard & Statistics
  VIEW_ADMIN_DASHBOARD = 'view_admin_dashboard',
  VIEW_STATISTICS = 'view_statistics',
  EXPORT_REPORTS = 'export_reports',
  
  // Own Profile
  VIEW_OWN_PROFILE = 'view_own_profile',
  UPDATE_OWN_PROFILE = 'update_own_profile',
  
  // Goals
  VIEW_OWN_GOALS = 'view_own_goals',
  CREATE_OWN_GOALS = 'create_own_goals',
  UPDATE_OWN_GOALS = 'update_own_goals',
  DELETE_OWN_GOALS = 'delete_own_goals',
  
  // Workout Logs
  VIEW_OWN_WORKOUTS = 'view_own_workouts',
  CREATE_OWN_WORKOUTS = 'create_own_workouts',
  UPDATE_OWN_WORKOUTS = 'update_own_workouts',
  DELETE_OWN_WORKOUTS = 'delete_own_workouts',
  
  // Nutrition Logs
  VIEW_OWN_NUTRITION = 'view_own_nutrition',
  CREATE_OWN_NUTRITION = 'create_own_nutrition',
  UPDATE_OWN_NUTRITION = 'update_own_nutrition',
  DELETE_OWN_NUTRITION = 'delete_own_nutrition',
}

export interface RolePermissions {
  role: Role;
  permissions: Permission[];
}

// Define permissions for each role
export const ROLE_PERMISSIONS: Record<Role, Permission[]> = {
  [Role.ADMIN]: [
    // Admin has all permissions
    Permission.VIEW_USERS,
    Permission.CREATE_USERS,
    Permission.UPDATE_USERS,
    Permission.DELETE_USERS,
    Permission.UPDATE_USER_ROLES,
    Permission.VIEW_EXERCISES,
    Permission.CREATE_EXERCISES,
    Permission.UPDATE_EXERCISES,
    Permission.DELETE_EXERCISES,
    Permission.VIEW_FOODS,
    Permission.CREATE_FOODS,
    Permission.UPDATE_FOODS,
    Permission.DELETE_FOODS,
    Permission.VIEW_ADMIN_DASHBOARD,
    Permission.VIEW_STATISTICS,
    Permission.EXPORT_REPORTS,
    Permission.VIEW_OWN_PROFILE,
    Permission.UPDATE_OWN_PROFILE,
  ],
  [Role.USER]: [
    // User has limited permissions (only their own data)
    Permission.VIEW_OWN_PROFILE,
    Permission.UPDATE_OWN_PROFILE,
    Permission.VIEW_OWN_GOALS,
    Permission.CREATE_OWN_GOALS,
    Permission.UPDATE_OWN_GOALS,
    Permission.DELETE_OWN_GOALS,
    Permission.VIEW_OWN_WORKOUTS,
    Permission.CREATE_OWN_WORKOUTS,
    Permission.UPDATE_OWN_WORKOUTS,
    Permission.DELETE_OWN_WORKOUTS,
    Permission.VIEW_OWN_NUTRITION,
    Permission.CREATE_OWN_NUTRITION,
    Permission.UPDATE_OWN_NUTRITION,
    Permission.DELETE_OWN_NUTRITION,
    Permission.VIEW_EXERCISES,
    Permission.VIEW_FOODS,
  ],
};

export interface PermissionCheck {
  hasPermission: boolean;
  reason?: string;
}
