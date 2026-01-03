import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, useLocation } from "react-router-dom";
import { AnimatePresence, motion } from "framer-motion";
import { AuthProvider } from "@/contexts/AuthContext";
import { Permission } from "@/types/rbac";
import HomeGuest from "./pages/HomeGuest";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import GoogleCallback from "./pages/GoogleCallback";
import CreatePasswordForGoogle from "./pages/CreatePasswordForGoogle";
import VerifyEmail from "./pages/VerifyEmail";
import RegisterSuccess from "./pages/RegisterSuccess";
import ForgotPassword from "./pages/ForgotPassword";
import VerifyPasswordReset from "./pages/VerifyPasswordReset";
import ResetPassword from "./pages/ResetPassword";
import ResetSuccess from "./pages/ResetSuccess";
import ChangePasswordSuccess from "./pages/ChangePasswordSuccess";
import CompleteProfile from "./pages/CompleteProfile";
import NotFound from "./pages/NotFound";
import AdminDashboard from "./pages/admin/AdminDashboard";
import Profile from "./pages/Profile";
import UserManagement from "./pages/admin/UserManagement";
import ExerciseManagement from "./pages/admin/ExerciseManagement";
import FoodManagement from "./pages/admin/FoodManagement";
import ProtectedRoute from "./components/ProtectedRoute";
import GoalsPage from "./pages/GoalsPage";
import CreateGoalPage from "./pages/CreateGoalPage";
import AddProgressPage from "./pages/AddProgressPage";
import GoalDetailsPage from "./pages/GoalDetailsPage";
import NutritionPage from "./pages/NutritionPage";
import CreateWorkoutPage from "./pages/CreateWorkoutPage";
import WorkoutHistoryPage from "./pages/WorkoutHistoryPage";
import FoodList from "./pages/FoodList";
import FoodSearch from "./pages/FoodSearch";
import ExerciseLibraryPage from "./pages/ExerciseLibraryPage";

const queryClient = new QueryClient();

function AppContent() {
  const location = useLocation();

  return (
    <AnimatePresence mode="wait">
      <Routes location={location} key={location.pathname}>
        <Route path="/" element={
          <motion.div
            initial={{ opacity: 0, x: -50 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 50 }}
            transition={{ duration: 0.5 }}
          >
            <HomeGuest />
          </motion.div>
        } />
        <Route path="/login" element={
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 1.1 }}
            transition={{ duration: 0.4 }}
          >
            <Login />
          </motion.div>
        } />
        <Route path="/register" element={
          <motion.div
            initial={{ opacity: 0, y: -50 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 50 }}
            transition={{ duration: 0.5 }}
          >
            <Register />
          </motion.div>
        } />
        <Route path="/verify-email" element={
          <motion.div
            initial={{ opacity: 0, rotate: -10 }}
            animate={{ opacity: 1, rotate: 0 }}
            exit={{ opacity: 0, rotate: 10 }}
            transition={{ duration: 0.4 }}
          >
            <VerifyEmail />
          </motion.div>
        } />
        <Route path="/register-success" element={
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 1.2 }}
            transition={{ duration: 0.5 }}
          >
            <RegisterSuccess />
          </motion.div>
        } />
        <Route path="/forgot-password" element={
          <motion.div
            initial={{ opacity: 0, x: 50 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -50 }}
            transition={{ duration: 0.5 }}
          >
            <ForgotPassword />
          </motion.div>
        } />
        <Route path="/verify-password-reset" element={
          <motion.div
            initial={{ opacity: 0, y: 50 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -50 }}
            transition={{ duration: 0.4 }}
          >
            <VerifyPasswordReset />
          </motion.div>
        } />
        <Route path="/reset-password" element={
          <motion.div
            initial={{ opacity: 0, scale: 1.1 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.9 }}
            transition={{ duration: 0.5 }}
          >
            <ResetPassword />
          </motion.div>
        } />
        <Route path="/reset-success" element={
          <motion.div
            initial={{ opacity: 0, rotate: 10 }}
            animate={{ opacity: 1, rotate: 0 }}
            exit={{ opacity: 0, rotate: -10 }}
            transition={{ duration: 0.4 }}
          >
            <ResetSuccess />
          </motion.div>
        } />
        <Route path="/change-password-success" element={
          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 1.2 }}
            transition={{ duration: 0.5 }}
          >
            <ChangePasswordSuccess />
          </motion.div>
        } />
        <Route path="/create-password-google" element={
          <motion.div
            initial={{ opacity: 0, y: -50 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 50 }}
            transition={{ duration: 0.5 }}
          >
            <CreatePasswordForGoogle />
          </motion.div>
        } />
        <Route path="/google/callback" element={
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <GoogleCallback />
          </motion.div>
        } />
        <Route path="/complete-profile" element={
          <motion.div
            initial={{ opacity: 0, y: -50 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 50 }}
            transition={{ duration: 0.5 }}
          >
            <CompleteProfile />
          </motion.div>
        } />
        <Route path="/dashboard" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: -100 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 100 }}
              transition={{ duration: 0.6 }}
            >
              <Dashboard />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/profile" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 1.05 }}
              transition={{ duration: 0.4 }}
            >
              <Profile />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/admin/dashboard" element={
          <ProtectedRoute
            requireAdmin={true}
            requiredPermission={Permission.VIEW_ADMIN_DASHBOARD}
          >
            <motion.div
              initial={{ opacity: 0, x: -100 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 100 }}
              transition={{ duration: 0.6 }}
            >
              <AdminDashboard />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/admin/users" element={
          <ProtectedRoute
            requireAdmin={true}
            requiredPermission={Permission.VIEW_USERS}
          >
            <motion.div
              initial={{ opacity: 0, x: -100 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 100 }}
              transition={{ duration: 0.6 }}
            >
              <UserManagement />
            </motion.div>
          </ProtectedRoute>
        } />

        <Route path="/admin/exercises" element={
          <ProtectedRoute
            requireAdmin={true}
            requiredPermission={Permission.VIEW_EXERCISES}
          >
            <motion.div
              initial={{ opacity: 0, x: -100 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 100 }}
              transition={{ duration: 0.6 }}
            >
              <ExerciseManagement />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/admin/foods" element={
          <ProtectedRoute
            requireAdmin={true}
            requiredPermission={Permission.VIEW_FOODS}
          >
            <motion.div
              initial={{ opacity: 0, x: -100 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: 100 }}
              transition={{ duration: 0.6 }}
            >
              <FoodManagement />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/goals" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              transition={{ duration: 0.4 }}
            >
              <GoalsPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/goals/create" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <CreateGoalPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/goals/:goalId" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 1.05 }}
              transition={{ duration: 0.4 }}
            >
              <GoalDetailsPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/goals/:goalId/progress" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, y: 50 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -50 }}
              transition={{ duration: 0.4 }}
            >
              <AddProgressPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/nutrition" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <NutritionPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/nutrition/search" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <FoodSearch />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/nutrition/list" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <FoodList />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/create-workout" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <CreateWorkoutPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/exercises" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <ExerciseLibraryPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="/workout-history" element={
          <ProtectedRoute>
            <motion.div
              initial={{ opacity: 0, x: 50 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -50 }}
              transition={{ duration: 0.4 }}
            >
              <WorkoutHistoryPage />
            </motion.div>
          </ProtectedRoute>
        } />
        <Route path="*" element={
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <NotFound />
          </motion.div>
        } />
      </Routes>
    </AnimatePresence>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <TooltipProvider>
          <Toaster />
          <Sonner />
          <BrowserRouter>
            <AppContent />
          </BrowserRouter>
        </TooltipProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}
