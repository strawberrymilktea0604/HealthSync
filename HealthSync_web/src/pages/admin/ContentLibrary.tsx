import { useState, useEffect } from "react";
import AdminLayout from "@/components/admin/AdminLayout";
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { Tag } from 'primereact/tag';
import { TabView, TabPanel } from 'primereact/tabview';
import { exerciseService, Exercise, CreateExerciseDto, UpdateExerciseDto } from "@/services/exerciseService";
import { foodItemService, FoodItem, CreateFoodItemDto, UpdateFoodItemDto } from "@/services/foodItemService";
import { toast } from "sonner";
import { Can } from "@/components/PermissionGuard";
import { usePermissionCheck } from "@/hooks/usePermissionCheck";
import { Permission } from "@/types/rbac";
import 'primeflex/primeflex.css';

export default function ContentLibrary() {
  const [activeTab, setActiveTab] = useState<number>(0);
  const [searchTerm, setSearchTerm] = useState("");
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [foods, setFoods] = useState<FoodItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingExercise, setEditingExercise] = useState<Exercise | null>(null);
  const [editingFood, setEditingFood] = useState<FoodItem | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [deletingExercise, setDeletingExercise] = useState<Exercise | null>(null);
  const [deletingFood, setDeletingFood] = useState<FoodItem | null>(null);
  const { checkAndExecute } = usePermissionCheck();
  
  // Exercise Form states
  const [exerciseFormData, setExerciseFormData] = useState<CreateExerciseDto>({
    name: "",
    muscleGroup: "",
    difficulty: "Beginner",
    equipment: "",
    description: "",
  });

  // Food Form states
  const [foodFormData, setFoodFormData] = useState<CreateFoodItemDto>({
    name: "",
    servingSize: 0,
    servingUnit: "g",
    caloriesKcal: 0,
    proteinG: 0,
    carbsG: 0,
    fatG: 0,
  });

  // Load data on mount and tab change
  useEffect(() => {
    if (activeTab === 0) {
      fetchExercises();
    } else {
      fetchFoods();
    }
  }, [activeTab]);

  const fetchExercises = async () => {
    try {
      setLoading(true);
      const data = await exerciseService.getExercises();
      setExercises(data);
    } catch (error: unknown) {
      console.error("Error fetching exercises:", error);
      toast.error("Không thể tải danh sách bài tập");
    } finally {
      setLoading(false);
    }
  };

  const fetchFoods = async () => {
    try {
      setLoading(true);
      const data = await foodItemService.getFoodItems();
      setFoods(data);
    } catch (error: unknown) {
      console.error("Error fetching foods:", error);
      toast.error("Không thể tải danh sách món ăn");
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = (item?: Exercise | FoodItem) => {
    if (activeTab === 0) {
      const exercise = item;
      if (exercise) {
        setEditingExercise(exercise);
        setExerciseFormData({
          name: exercise.name,
          muscleGroup: exercise.muscleGroup,
          difficulty: exercise.difficulty,
          equipment: exercise.equipment || "",
          description: exercise.description || "",
        });
      } else {
        setEditingExercise(null);
        setExerciseFormData({
          name: "",
          muscleGroup: "",
          difficulty: "Beginner",
          equipment: "",
          description: "",
        });
      }
    } else {
      const food = item;
      if (food) {
        setEditingFood(food);
        setFoodFormData({
          name: food.name,
          servingSize: food.servingSize,
          servingUnit: food.servingUnit,
          caloriesKcal: food.caloriesKcal,
          proteinG: food.proteinG,
          carbsG: food.carbsG,
          fatG: food.fatG,
        });
      } else {
        setEditingFood(null);
        setFoodFormData({
          name: "",
          servingSize: 0,
          servingUnit: "g",
          caloriesKcal: 0,
          proteinG: 0,
          carbsG: 0,
          fatG: 0,
        });
      }
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingExercise(null);
    setEditingFood(null);
    setExerciseFormData({
      name: "",
      muscleGroup: "",
      difficulty: "Beginner",
      equipment: "",
      description: "",
    });
    setFoodFormData({
      name: "",
      servingSize: 0,
      servingUnit: "g",
      caloriesKcal: 0,
      proteinG: 0,
      carbsG: 0,
      fatG: 0,
    });
  };

  const handleSubmitExercise = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!exerciseFormData.name || !exerciseFormData.muscleGroup) {
      toast.error("Vui lòng điền đầy đủ thông tin bắt buộc");
      return;
    }

    const permission = editingExercise ? Permission.UPDATE_EXERCISES : Permission.CREATE_EXERCISES;

    checkAndExecute(
      permission,
      async () => {
        try {
          setLoading(true);
          if (editingExercise) {
            const updateData: UpdateExerciseDto = {
              exerciseId: editingExercise.exerciseId,
              ...exerciseFormData,
            };
            await exerciseService.updateExercise(editingExercise.exerciseId, updateData);
            toast.success("Cập nhật bài tập thành công");
          } else {
            await exerciseService.createExercise(exerciseFormData);
            toast.success("Tạo bài tập mới thành công");
          }
          handleCloseModal();
          fetchExercises();
        } catch (error: unknown) {
          console.error("Error saving exercise:", error);
          const errorMessage = error && typeof error === 'object' && 'response' in error 
            ? (error as { response?: { data?: { message?: string } } }).response?.data?.message 
            : undefined;
          toast.error(errorMessage || "Có lỗi xảy ra");
        } finally {
          setLoading(false);
        }
      },
      { errorMessage: `Bạn không có quyền ${editingExercise ? 'cập nhật' : 'tạo'} bài tập` }
    );
  };

  const handleSubmitFood = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!foodFormData.name) {
      toast.error("Vui lòng điền tên món ăn");
      return;
    }

    const permission = editingFood ? Permission.UPDATE_FOOD_ITEMS : Permission.CREATE_FOOD_ITEMS;

    checkAndExecute(
      permission,
      async () => {
        try {
          setLoading(true);
          if (editingFood) {
            const updateData: UpdateFoodItemDto = {
              foodItemId: editingFood.foodItemId,
              ...foodFormData,
            };
            await foodItemService.updateFoodItem(editingFood.foodItemId, updateData);
            toast.success("Cập nhật món ăn thành công");
          } else {
            await foodItemService.createFoodItem(foodFormData);
            toast.success("Tạo món ăn mới thành công");
          }
          handleCloseModal();
          fetchFoods();
        } catch (error: unknown) {
          console.error("Error saving food:", error);
          const errorMessage = error && typeof error === 'object' && 'response' in error 
            ? (error as { response?: { data?: { message?: string } } }).response?.data?.message 
            : undefined;
          toast.error(errorMessage || "Có lỗi xảy ra");
        } finally {
          setLoading(false);
        }
      },
      { errorMessage: `Bạn không có quyền ${editingFood ? 'cập nhật' : 'tạo'} món ăn` }
    );
  };

  const handleDeleteExercise = async () => {
    if (!deletingExercise) return;

    checkAndExecute(
      Permission.DELETE_EXERCISES,
      async () => {
        try {
          await exerciseService.deleteExercise(deletingExercise.exerciseId);
          toast.success("Xóa bài tập thành công");
          setIsDeleteModalOpen(false);
          setDeletingExercise(null);
          fetchExercises();
        } catch {
          toast.error("Không thể xóa bài tập");
        }
      },
      { errorMessage: "Bạn không có quyền xóa bài tập" }
    );
  };

  const handleDeleteFood = async () => {
    if (!deletingFood) return;

    checkAndExecute(
      Permission.DELETE_FOOD_ITEMS,
      async () => {
        try {
          await foodItemService.deleteFoodItem(deletingFood.foodItemId);
          toast.success("Xóa món ăn thành công");
          setIsDeleteModalOpen(false);
          setDeletingFood(null);
          fetchFoods();
        } catch {
          toast.error("Không thể xóa món ăn");
        }
      },
      { errorMessage: "Bạn không có quyền xóa món ăn" }
    );
  };

  // Exercise Table Templates
  const exerciseNameTemplate = (rowData: Exercise) => {
    return <span className="font-medium text-900">{rowData.name}</span>;
  };

  const muscleGroupTemplate = (rowData: Exercise) => {
    return (
      <Tag 
        value={rowData.muscleGroup}
        style={{ backgroundColor: '#F5F5F5', color: '#3D3B30', borderRadius: '50px', padding: '0.25rem 0.75rem' }}
      />
    );
  };

  const difficultyTemplate = (rowData: Exercise) => {
    const getSeverity = (difficulty: string) => {
      switch (difficulty) {
        case 'Beginner': return 'success';
        case 'Intermediate': return 'warning';
        case 'Advanced': return 'danger';
        default: return 'info';
      }
    };

    return (
      <Tag 
        value={rowData.difficulty}
        severity={getSeverity(rowData.difficulty)}
        style={{ borderRadius: '50px', padding: '0.25rem 0.75rem' }}
      />
    );
  };

  const descriptionTemplate = (rowData: Exercise) => {
    return <span className="text-600 text-sm">{rowData.description || 'N/A'}</span>;
  };

  const exerciseActionsTemplate = (rowData: Exercise) => {
    return (
      <div className="flex gap-2">
        <Can permission={Permission.UPDATE_EXERCISES}>
          <Button
            icon="pi pi-pencil"
            size="small"
            rounded
            outlined
            style={{ width: '2rem', height: '2rem' }}
            onClick={() => handleOpenModal(rowData)}
          />
        </Can>
        <Can permission={Permission.DELETE_EXERCISES}>
          <Button
            icon="pi pi-trash"
            size="small"
            rounded
            outlined
            severity="danger"
            style={{ width: '2rem', height: '2rem' }}
            onClick={() => {
              setDeletingExercise(rowData);
              setIsDeleteModalOpen(true);
            }}
          />
        </Can>
      </div>
    );
  };

  // Food Table Templates
  const foodNameTemplate = (rowData: FoodItem) => {
    return <span className="font-medium text-900">{rowData.name}</span>;
  };

  const servingSizeTemplate = (rowData: FoodItem) => {
    return <span>{rowData.servingSize} {rowData.servingUnit}</span>;
  };

  const caloriesTemplate = (rowData: FoodItem) => {
    return <span>{rowData.caloriesKcal} kcal</span>;
  };

  const macrosTemplate = (rowData: FoodItem) => {
    return (
      <div className="text-sm">
        <div>P: {rowData.proteinG}g</div>
        <div>C: {rowData.carbsG}g</div>
        <div>F: {rowData.fatG}g</div>
      </div>
    );
  };

  const foodActionsTemplate = (rowData: FoodItem) => {
    return (
      <div className="flex gap-2">
        <Can permission={Permission.UPDATE_FOOD_ITEMS}>
          <Button
            icon="pi pi-pencil"
            size="small"
            rounded
            outlined
            style={{ width: '2rem', height: '2rem' }}
            onClick={() => handleOpenModal(rowData)}
          />
        </Can>
        <Can permission={Permission.DELETE_FOOD_ITEMS}>
          <Button
            icon="pi pi-trash"
            size="small"
            rounded
            outlined
            severity="danger"
            style={{ width: '2rem', height: '2rem' }}
            onClick={() => {
              setDeletingFood(rowData);
              setIsDeleteModalOpen(true);
            }}
          />
        </Can>
      </div>
    );
  };

  const difficultyOptions = [
    { label: 'Beginner', value: 'Beginner' },
    { label: 'Intermediate', value: 'Intermediate' },
    { label: 'Advanced', value: 'Advanced' }
  ];

  const muscleGroupOptions = [
    { label: 'Chest', value: 'Chest' },
    { label: 'Back', value: 'Back' },
    { label: 'Legs', value: 'Legs' },
    { label: 'Shoulders', value: 'Shoulders' },
    { label: 'Arms', value: 'Arms' },
    { label: 'Core', value: 'Core' }
  ];

  return (
    <AdminLayout>
      <div className="p-4">
        {/* Header */}
        <div className="surface-card border-round-xl shadow-2 p-4 mb-4">
          <div className="flex flex-column md:flex-row gap-3 align-items-start md:align-items-center justify-content-between">
            <div className="flex-1 w-full md:w-auto">
              <span className="p-input-icon-left w-full">
                <i className="pi pi-search" />
                <InputText
                  placeholder={activeTab === 0 ? "Search exercises..." : "Search food items..."}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full"
                  style={{ paddingLeft: '2.5rem', borderRadius: '8px' }}
                />
              </span>
            </div>
            <Button
              label={activeTab === 0 ? "+ Add New Exercise" : "+ Add New Food Item"}
              icon="pi pi-plus"
              className="w-full md:w-auto"
              style={{ backgroundColor: '#4A6C6F', border: 'none', borderRadius: '8px' }}
              onClick={() => handleOpenModal()}
            />
          </div>
        </div>

        {/* Tabs */}
        <div className="surface-card border-round-xl shadow-2">
          <TabView activeIndex={activeTab} onTabChange={(e) => setActiveTab(e.index)}>
            <TabPanel header="Exercises" leftIcon="pi pi-fw pi-users">
              <DataTable
                value={exercises.filter(ex => ex.name.toLowerCase().includes(searchTerm.toLowerCase()))}
                loading={loading}
                paginator
                rows={10}
                dataKey="exerciseId"
                emptyMessage="No exercises found"
              >
                <Column 
                  header="EXERCISE NAME" 
                  body={exerciseNameTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="TARGET MUSCLE" 
                  body={muscleGroupTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="DIFFICULTY" 
                  body={difficultyTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="DESCRIPTION" 
                  body={descriptionTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="ACTIONS" 
                  body={exerciseActionsTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
              </DataTable>
            </TabPanel>

            <TabPanel header="Food Items" leftIcon="pi pi-fw pi-shopping-cart">
              <DataTable
                value={foods.filter(food => food.name.toLowerCase().includes(searchTerm.toLowerCase()))}
                loading={loading}
                paginator
                rows={10}
                dataKey="foodItemId"
                emptyMessage="No food items found"
              >
                <Column 
                  header="FOOD NAME" 
                  body={foodNameTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="SERVING SIZE" 
                  body={servingSizeTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="CALORIES" 
                  body={caloriesTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="MACROS" 
                  body={macrosTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
                <Column 
                  header="ACTIONS" 
                  body={foodActionsTemplate}
                  headerStyle={{ 
                    backgroundColor: '#F5F5F5', 
                    color: '#3D3B30', 
                    textTransform: 'uppercase', 
                    fontSize: '0.875rem', 
                    fontWeight: 600 
                  }}
                />
              </DataTable>
            </TabPanel>
          </TabView>
        </div>
      </div>

      {/* Exercise Form Dialog */}
      <Dialog
        visible={isModalOpen && activeTab === 0}
        onHide={handleCloseModal}
        header={editingExercise ? "Edit Exercise" : "Add New Exercise"}
        style={{ width: '600px' }}
        modal
      >
        <form onSubmit={handleSubmitExercise} className="flex flex-column gap-4">
          <div className="flex flex-column gap-2">
            <label htmlFor="name" className="font-semibold text-sm text-900">
              EXERCISE NAME *
            </label>
            <InputText
              id="name"
              placeholder="e.g., Barbell Bench Press"
              value={exerciseFormData.name}
              onChange={(e) => setExerciseFormData({ ...exerciseFormData, name: e.target.value })}
              className="w-full"
              required
            />
          </div>

          <div className="flex flex-column gap-2">
            <label htmlFor="muscleGroup" className="font-semibold text-sm text-900">
              TARGET MUSCLE GROUP *
            </label>
            <Dropdown
              id="muscleGroup"
              value={exerciseFormData.muscleGroup}
              options={muscleGroupOptions}
              onChange={(e) => setExerciseFormData({ ...exerciseFormData, muscleGroup: e.value })}
              placeholder="Select muscle group"
              className="w-full"
            />
          </div>

          <div className="flex flex-column gap-2">
            <label htmlFor="difficulty" className="font-semibold text-sm text-900">
              DIFFICULTY LEVEL *
            </label>
            <Dropdown
              id="difficulty"
              value={exerciseFormData.difficulty}
              options={difficultyOptions}
              onChange={(e) => setExerciseFormData({ ...exerciseFormData, difficulty: e.value })}
              placeholder="Select difficulty"
              className="w-full"
            />
          </div>

          <div className="flex flex-column gap-2">
            <label htmlFor="description" className="font-semibold text-sm text-900">
              INSTRUCTIONS
            </label>
            <InputTextarea
              id="description"
              rows={5}
              value={exerciseFormData.description}
              onChange={(e) => setExerciseFormData({ ...exerciseFormData, description: e.target.value })}
              className="w-full"
            />
          </div>

          <div className="flex justify-content-end gap-2 pt-3" style={{ borderTop: '1px solid #E5E5E5' }}>
            <Button
              label="Cancel"
              type="button"
              outlined
              onClick={handleCloseModal}
              style={{ borderRadius: '8px' }}
            />
            <Button
              label="Save Exercise"
              type="submit"
              loading={loading}
              style={{ backgroundColor: '#4A6C6F', border: 'none', borderRadius: '8px' }}
            />
          </div>
        </form>
      </Dialog>

      {/* Food Form Dialog */}
      <Dialog
        visible={isModalOpen && activeTab === 1}
        onHide={handleCloseModal}
        header={editingFood ? "Edit Food Item" : "Add New Food Item"}
        style={{ width: '600px' }}
        modal
      >
        <form onSubmit={handleSubmitFood} className="flex flex-column gap-4">
          <div className="flex flex-column gap-2">
            <label htmlFor="foodName" className="font-semibold text-sm">
              Food Name *
            </label>
            <InputText
              id="foodName"
              value={foodFormData.name}
              onChange={(e) => setFoodFormData({ ...foodFormData, name: e.target.value })}
              className="w-full"
              required
            />
          </div>

          <div className="grid">
            <div className="col-6 flex flex-column gap-2">
              <label htmlFor="servingSize" className="font-semibold text-sm">
                Serving Size *
              </label>
              <InputText
                id="servingSize"
                type="number"
                value={foodFormData.servingSize.toString()}
                onChange={(e) => setFoodFormData({ ...foodFormData, servingSize: Number.parseFloat(e.target.value) })}
                className="w-full"
                required
              />
            </div>

            <div className="col-6 flex flex-column gap-2">
              <label htmlFor="servingUnit" className="font-semibold text-sm">
                Unit *
              </label>
              <InputText
                id="servingUnit"
                value={foodFormData.servingUnit}
                onChange={(e) => setFoodFormData({ ...foodFormData, servingUnit: e.target.value })}
                className="w-full"
                required
              />
            </div>
          </div>

          <div className="flex flex-column gap-2">
            <label htmlFor="calories" className="font-semibold text-sm">
              Calories (kcal) *
            </label>
            <InputText
              id="calories"
              type="number"
              value={foodFormData.caloriesKcal.toString()}
              onChange={(e) => setFoodFormData({ ...foodFormData, caloriesKcal: Number.parseFloat(e.target.value) })}
              className="w-full"
              required
            />
          </div>

          <div className="grid">
            <div className="col-4 flex flex-column gap-2">
              <label htmlFor="protein" className="font-semibold text-sm">
                Protein (g)
              </label>
              <InputText
                id="protein"
                type="number"
                step="0.1"
                value={foodFormData.proteinG.toString()}
                onChange={(e) => setFoodFormData({ ...foodFormData, proteinG: Number.parseFloat(e.target.value) })}
                className="w-full"
              />
            </div>

            <div className="col-4 flex flex-column gap-2">
              <label htmlFor="carbs" className="font-semibold text-sm">
                Carbs (g)
              </label>
              <InputText
                id="carbs"
                type="number"
                step="0.1"
                value={foodFormData.carbsG.toString()}
                onChange={(e) => setFoodFormData({ ...foodFormData, carbsG: Number.parseFloat(e.target.value) })}
                className="w-full"
              />
            </div>

            <div className="col-4 flex flex-column gap-2">
              <label htmlFor="fat" className="font-semibold text-sm">
                Fat (g)
              </label>
              <InputText
                id="fat"
                type="number"
                step="0.1"
                value={foodFormData.fatG.toString()}
                onChange={(e) => setFoodFormData({ ...foodFormData, fatG: Number.parseFloat(e.target.value) })}
                className="w-full"
              />
            </div>
          </div>

          <div className="flex justify-content-end gap-2 pt-3" style={{ borderTop: '1px solid #E5E5E5' }}>
            <Button
              label="Cancel"
              type="button"
              outlined
              onClick={handleCloseModal}
            />
            <Button
              label="Save Food Item"
              type="submit"
              loading={loading}
              style={{ backgroundColor: '#4A6C6F', border: 'none' }}
            />
          </div>
        </form>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        visible={isDeleteModalOpen}
        onHide={() => {
          setIsDeleteModalOpen(false);
          setDeletingExercise(null);
          setDeletingFood(null);
        }}
        header="Delete Confirmation"
        style={{ width: '450px' }}
        modal
      >
        <div className="flex flex-column gap-4">
          <p className="text-600 line-height-3">
            Are you sure you want to delete this {activeTab === 0 ? 'exercise' : 'food item'}? This action cannot be undone.
          </p>
          <div className="flex justify-content-end gap-2">
            <Button
              label="Cancel"
              outlined
              onClick={() => {
                setIsDeleteModalOpen(false);
                setDeletingExercise(null);
                setDeletingFood(null);
              }}
            />
            <Button
              label="Delete"
              severity="danger"
              onClick={activeTab === 0 ? handleDeleteExercise : handleDeleteFood}
            />
          </div>
        </div>
      </Dialog>
    </AdminLayout>
  );
}
