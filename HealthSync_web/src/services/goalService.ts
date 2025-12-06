import api from './api';

export interface CreateGoalRequest {
  type: string;
  targetValue: number;
  startDate: string;
  endDate?: string;
  notes?: string;
}

export interface ProgressRecord {
  progressRecordId: number;
  recordDate: string;
  value: number;
  notes?: string;
  weightKg: number;
  waistCm: number;
}

export interface Goal {
  goalId: number;
  type: string;
  targetValue: number;
  startDate: string;
  endDate?: string;
  status: string;
  notes?: string;
  progressRecords: ProgressRecord[];
}

export interface AddProgressRequest {
  recordDate: string;
  value: number;
  notes?: string;
  weightKg?: number;
  waistCm?: number;
}

export const goalService = {
  async createGoal(data: CreateGoalRequest): Promise<Goal> {
    const response = await api.post('/goals', data);
    return response.data;
  },

  async getGoals(): Promise<Goal[]> {
    const response = await api.get('/goals');
    return response.data.goals;
  },

  async addProgress(goalId: number, data: AddProgressRequest): Promise<ProgressRecord> {
    const response = await api.post(`/goals/${goalId}/progress`, data);
    return response.data.progressRecord;
  },
};
