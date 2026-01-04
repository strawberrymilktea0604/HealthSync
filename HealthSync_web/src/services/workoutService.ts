import api from './api';

export interface Exercise {
  exerciseId: number;
  name: string;
  muscleGroup: string;
  difficulty: string;
  equipment?: string;
  description?: string;
  imageUrl?: string;
}

export interface ExerciseSession {
  exerciseId: number;
  sets: number;
  reps: number;
  weightKg: number;
  restSec?: number;
  rpe?: number;
}

export interface CreateWorkoutLog {
  workoutDate: string;
  durationMin: number;
  notes?: string;
  exerciseSessions: ExerciseSession[];
}

export interface WorkoutLog {
  workoutLogId: number;
  userId: number;
  workoutDate: string;
  durationMin: number;
  notes?: string;
  exerciseSessions: {
    exerciseSessionId: number;
    exerciseId: number;
    exerciseName: string;
    sets: number;
    reps: number;
    weightKg: number;
    restSec?: number;
    rpe?: number;
  }[];
}

export const workoutService = {
  getExercises: async (filters?: { muscleGroup?: string; difficulty?: string; search?: string }) => {
    const params = new URLSearchParams();
    if (filters?.muscleGroup) params.append('muscleGroup', filters.muscleGroup);
    if (filters?.difficulty) params.append('difficulty', filters.difficulty);
    if (filters?.search) params.append('search', filters.search);

    const response = await api.get<Exercise[]>(`/workout/exercises?${params.toString()}`);
    return response.data;
  },

  getWorkoutLogs: async (startDate?: string, endDate?: string) => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await api.get<WorkoutLog[]>(`/workout/workout-logs?${params.toString()}`);
    return response.data;
  },

  createWorkoutLog: async (workoutLog: CreateWorkoutLog) => {
    const response = await api.post<{ workoutLogId: number }>('/workout/workout-logs', workoutLog);
    return response.data;
  }
};
