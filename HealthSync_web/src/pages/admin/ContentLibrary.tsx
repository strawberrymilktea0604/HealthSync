import { useState } from "react";
import AdminLayout from "@/components/admin/AdminLayout";
import Table from "@/components/admin/Table";
import Button from "@/components/admin/Button";
import Badge from "@/components/admin/Badge";
import Input from "@/components/admin/Input";

interface Exercise {
  id: string;
  name: string;
  muscleGroup: string;
  difficulty: string;
  description: string;
}

interface FoodItem {
  id: string;
  name: string;
  servingSize: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

export default function ContentLibrary() {
  const [activeTab, setActiveTab] = useState<"exercises" | "foods">(
    "exercises"
  );
  const [searchTerm, setSearchTerm] = useState("");

  const exercises: Exercise[] = [
    {
      id: "EX001",
      name: "Push-ups",
      muscleGroup: "Chest",
      difficulty: "Beginner",
      description: "Classic bodyweight exercise",
    },
    {
      id: "EX002",
      name: "Squats",
      muscleGroup: "Legs",
      difficulty: "Beginner",
      description: "Lower body strength exercise",
    },
    {
      id: "EX003",
      name: "Deadlifts",
      muscleGroup: "Back",
      difficulty: "Advanced",
      description: "Compound strength movement",
    },
    {
      id: "EX004",
      name: "Plank",
      muscleGroup: "Core",
      difficulty: "Intermediate",
      description: "Core stability exercise",
    },
    {
      id: "EX005",
      name: "Pull-ups",
      muscleGroup: "Back",
      difficulty: "Advanced",
      description: "Upper body pulling exercise",
    },
    {
      id: "EX006",
      name: "Lunges",
      muscleGroup: "Legs",
      difficulty: "Intermediate",
      description: "Single leg strength exercise",
    },
  ];

  const foods: FoodItem[] = [];

  const exerciseColumns = [
    {
      header: "EXERCISE NAME",
      accessor: "name" as keyof Exercise,
    },
    {
      header: "TARGET MUSCLE",
      accessor: "muscleGroup" as keyof Exercise,
    },
    {
      header: "DIFFICULTY",
      accessor: (row: Exercise) => {
        const variant =
          row.difficulty === "Beginner"
            ? "success"
            : row.difficulty === "Intermediate"
            ? "warning"
            : "danger";
        return <Badge variant={variant}>{row.difficulty}</Badge>;
      },
    },
    {
      header: "DESCRIPTION",
      accessor: "description" as keyof Exercise,
    },
    {
      header: "ACTIONS",
      accessor: () => (
        <div className="flex gap-2">
          <Button size="sm" variant="primary">
            ✏️
          </Button>
          <Button size="sm" variant="danger">
            🗑️
          </Button>
        </div>
      ),
    },
  ];

  const foodColumns = [
    {
      header: "FOOD NAME",
      accessor: "name" as keyof FoodItem,
    },
    {
      header: "SERVING SIZE",
      accessor: "servingSize" as keyof FoodItem,
    },
    {
      header: "CALORIES",
      accessor: (row: FoodItem) => `${row.calories} kcal`,
    },
    {
      header: "PROTEIN",
      accessor: (row: FoodItem) => `${row.protein}g`,
    },
    {
      header: "CARBS",
      accessor: (row: FoodItem) => `${row.carbs}g`,
    },
    {
      header: "FAT",
      accessor: (row: FoodItem) => `${row.fat}g`,
    },
    {
      header: "ACTIONS",
      accessor: () => (
        <div className="flex gap-2">
          <Button size="sm" variant="primary">
            ✏️
          </Button>
          <Button size="sm" variant="danger">
            🗑️
          </Button>
        </div>
      ),
    },
  ];

  return (
    <AdminLayout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-3xl font-bold text-gray-900">
              Content Library
            </h2>
            <p className="text-gray-600 mt-1">
              Manage exercises and food items
            </p>
          </div>
          <Button size="lg" variant="primary">
            + Add New {activeTab === "exercises" ? "Exercise" : "Food Item"}
          </Button>
        </div>

        <div className="bg-white rounded-lg shadow">
          <div className="border-b border-gray-200">
            <div className="flex">
              <button
                onClick={() => setActiveTab("exercises")}
                className={`px-6 py-4 font-medium transition-colors ${
                  activeTab === "exercises"
                    ? "text-[#4A6F6F] border-b-2 border-[#4A6F6F]"
                    : "text-gray-500 hover:text-gray-700"
                }`}
              >
                Exercises
              </button>
              <button
                onClick={() => setActiveTab("foods")}
                className={`px-6 py-4 font-medium transition-colors ${
                  activeTab === "foods"
                    ? "text-[#4A6F6F] border-b-2 border-[#4A6F6F]"
                    : "text-gray-500 hover:text-gray-700"
                }`}
              >
                Food Items
              </button>
            </div>
          </div>

          <div className="p-6">
            <div className="mb-6">
              <Input
                type="text"
                placeholder={`Search ${
                  activeTab === "exercises" ? "exercises" : "food items"
                }...`}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            {activeTab === "exercises" ? (
              <Table data={exercises} columns={exerciseColumns} />
            ) : (
              <Table data={foods} columns={foodColumns} />
            )}
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
