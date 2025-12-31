import { useEffect, useState, useRef } from 'react';

import AdminLayout from "@/components/admin/AdminLayout";
import { Card } from 'primereact/card';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Dialog } from 'primereact/dialog';
import { FileUpload } from 'primereact/fileupload';
import { foodItemService, FoodItem, UpdateFoodItemDto } from '@/services/foodItemService';
import { toast } from 'sonner';
import 'primeflex/primeflex.css';

export default function FoodManagement() {
  const [foods, setFoods] = useState<FoodItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedFood, setSelectedFood] = useState<FoodItem | null>(null);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [showImageDialog, setShowImageDialog] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);

  const fileUploadRef = useRef<FileUpload>(null);

  const [formData, setFormData] = useState({
    name: '',
    servingSize: 0,
    servingUnit: 'g',
    caloriesKcal: 0,
    proteinG: 0,
    carbsG: 0,
    fatG: 0
  });

  useEffect(() => {
    fetchFoods();
  }, []);

  const fetchFoods = async () => {
    try {
      setLoading(true);
      const data = await foodItemService.getFoodItems();
      setFoods(data);
    } catch (error: unknown) {
      console.error('Error fetching foods:', error);
      toast.error('Không thể tải danh sách thực phẩm');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await foodItemService.createFoodItem(formData);
      toast.success('Tạo thực phẩm thành công');
      setShowCreateDialog(false);
      setFormData({
        name: '',
        servingSize: 0,
        servingUnit: 'g',
        caloriesKcal: 0,
        proteinG: 0,
        carbsG: 0,
        fatG: 0
      });
      fetchFoods();
    } catch (error) {
      console.error('Error creating food:', error);
      toast.error('Không thể tạo thực phẩm');
    }
  };

  const handleEdit = async () => {
    if (!selectedFood) return;
    try {
      const updateData: UpdateFoodItemDto = {
        foodItemId: selectedFood.foodItemId,
        ...formData
      };
      await foodItemService.updateFoodItem(selectedFood.foodItemId, updateData);
      toast.success('Cập nhật thực phẩm thành công');
      setShowEditDialog(false);
      setSelectedFood(null);
      fetchFoods();
    } catch (error) {
      toast.error('Không thể cập nhật thực phẩm');
    }
  };

  const handleDelete = async (food: FoodItem) => {
    if (!confirm('Bạn có chắc muốn xóa thực phẩm này?')) return;
    try {
      await foodItemService.deleteFoodItem(food.foodItemId);
      toast.success('Xóa thực phẩm thành công');
      fetchFoods();
    } catch (error) {
      toast.error('Không thể xóa thực phẩm');
    }
  };

  const openEditDialog = (food: FoodItem) => {
    setSelectedFood(food);
    setFormData({
      name: food.name,
      servingSize: food.servingSize,
      servingUnit: food.servingUnit,
      caloriesKcal: food.caloriesKcal,
      proteinG: food.proteinG,
      carbsG: food.carbsG,
      fatG: food.fatG
    });
    setShowEditDialog(true);
  };

  const filteredFoods = foods.filter(food =>
    food.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const imageTemplate = (rowData: FoodItem) => {
    if (rowData.imageUrl) {
      return (
        <img
          src={`${rowData.imageUrl}?t=${Date.now()}`}
          alt={rowData.name}
          className="w-4rem h-4rem object-cover border-round"
        />
      );
    }
    return <span className="text-400">Chưa có ảnh</span>;
  };

  const handleImageUpload = async (event: { files: File[] }) => {
    if (!selectedFood || event.files.length === 0) return;

    try {
      setUploadingImage(true);
      const file = event.files[0];
      await foodItemService.uploadFoodItemImage(selectedFood.foodItemId, file);
      toast.success('Upload ảnh thành công');
      setShowImageDialog(false);
      setSelectedFood(null);
      await fetchFoods();
      if (fileUploadRef.current) {
        fileUploadRef.current.clear();
      }
    } catch (error) {
      toast.error('Không thể upload ảnh');
    } finally {
      setUploadingImage(false);
    }
  };

  const openImageDialog = (food: FoodItem) => {
    setSelectedFood(food);
    setShowImageDialog(true);
  };

  const actionTemplate = (rowData: FoodItem) => (
    <div className="flex gap-2">
      <Button
        icon="pi pi-image"
        className="p-button-rounded p-button-text p-button-success"
        onClick={() => openImageDialog(rowData)}
        tooltip="Upload ảnh"
      />
      <Button
        icon="pi pi-pencil"
        className="p-button-rounded p-button-text p-button-info"
        onClick={() => openEditDialog(rowData)}
      />
      <Button
        icon="pi pi-trash"
        className="p-button-rounded p-button-text p-button-danger"
        onClick={() => handleDelete(rowData)}
      />
    </div>
  );

  const nutritionTemplate = (rowData: FoodItem) => (
    <div className="text-sm">
      <div>Calories: {rowData.caloriesKcal} kcal</div>
      <div>Protein: {rowData.proteinG}g | Carbs: {rowData.carbsG}g | Fat: {rowData.fatG}g</div>
    </div>
  );

  return (
    <AdminLayout>
      <div className="p-4">
        <Card title="Quản lý Thực phẩm">
          <div className="flex justify-content-between align-items-center mb-4">
            <div className="flex gap-2">
              <InputText
                placeholder="Tìm kiếm thực phẩm..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-12rem"
              />
            </div>
            <Button
              label="Thêm thực phẩm"
              icon="pi pi-plus"
              onClick={() => setShowCreateDialog(true)}
              className="p-button-primary"
            />
          </div>

          <DataTable
            value={filteredFoods}
            loading={loading}
            paginator
            rows={10}
            rowsPerPageOptions={[5, 10, 25]}
            emptyMessage="Không có thực phẩm nào"
          >
            <Column field="name" header="Tên thực phẩm" sortable />
            <Column header="Ảnh" body={imageTemplate} style={{ width: '100px' }} />
            <Column field="servingSize" header="Khẩu phần" body={(row) => `${row.servingSize} ${row.servingUnit}`} sortable />
            <Column header="Dinh dưỡng" body={nutritionTemplate} />
            <Column header="Thao tác" body={actionTemplate} style={{ width: '150px' }} />
          </DataTable>
        </Card>

        {/* Image Upload Dialog */}
        <Dialog
          header="Upload ảnh món ăn"
          visible={showImageDialog}
          onHide={() => setShowImageDialog(false)}
          footer={
            <div>
              <Button label="Đóng" icon="pi pi-times" onClick={() => setShowImageDialog(false)} className="p-button-text" />
            </div>
          }
          style={{ width: '400px' }}
        >
          <FileUpload
            ref={fileUploadRef}
            name="image"
            accept="image/*"
            maxFileSize={5000000}
            customUpload
            uploadHandler={handleImageUpload}
            auto={false}
            chooseLabel="Chọn ảnh"
            uploadLabel="Upload"
            cancelLabel="Hủy"
            disabled={uploadingImage}
          />
          {uploadingImage && <p>Đang upload...</p>}
        </Dialog>

        {/* Create Dialog */}
        <Dialog
          header="Thêm thực phẩm mới"
          visible={showCreateDialog}
          onHide={() => setShowCreateDialog(false)}
          footer={
            <div>
              <Button label="Hủy" icon="pi pi-times" onClick={() => setShowCreateDialog(false)} className="p-button-text" />
              <Button label="Tạo" icon="pi pi-check" onClick={handleCreate} />
            </div>
          }
          style={{ width: '600px' }}
        >
          <div className="p-fluid">
            <div className="field">
              <label htmlFor="name">Tên thực phẩm</label>
              <InputText id="name" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} />
            </div>
            <div className="field">
              <label htmlFor="servingSize">Khẩu phần</label>
              <InputText id="servingSize" type="number" value={String(formData.servingSize)} onChange={(e) => setFormData({ ...formData, servingSize: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="servingUnit">Đơn vị</label>
              <InputText id="servingUnit" value={formData.servingUnit} onChange={(e) => setFormData({ ...formData, servingUnit: e.target.value })} />
            </div>
            <div className="field">
              <label htmlFor="caloriesKcal">Calories (kcal)</label>
              <InputText id="caloriesKcal" type="number" value={String(formData.caloriesKcal)} onChange={(e) => setFormData({ ...formData, caloriesKcal: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="proteinG">Protein (g)</label>
              <InputText id="proteinG" type="number" value={String(formData.proteinG)} onChange={(e) => setFormData({ ...formData, proteinG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="carbsG">Carbs (g)</label>
              <InputText id="carbsG" type="number" value={String(formData.carbsG)} onChange={(e) => setFormData({ ...formData, carbsG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="fatG">Fat (g)</label>
              <InputText id="fatG" type="number" value={String(formData.fatG)} onChange={(e) => setFormData({ ...formData, fatG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
          </div>
        </Dialog>

        {/* Edit Dialog */}
        <Dialog
          header="Chỉnh sửa thực phẩm"
          visible={showEditDialog}
          onHide={() => setShowEditDialog(false)}
          footer={
            <div>
              <Button label="Hủy" icon="pi pi-times" onClick={() => setShowEditDialog(false)} className="p-button-text" />
              <Button label="Lưu" icon="pi pi-check" onClick={handleEdit} />
            </div>
          }
          style={{ width: '600px' }}
        >
          <div className="p-fluid">
            <div className="field">
              <label htmlFor="edit-name">Tên thực phẩm</label>
              <InputText id="edit-name" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} />
            </div>
            <div className="field">
              <label htmlFor="edit-servingSize">Khẩu phần</label>
              <InputText id="edit-servingSize" type="number" value={String(formData.servingSize)} onChange={(e) => setFormData({ ...formData, servingSize: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="edit-servingUnit">Đơn vị</label>
              <InputText id="edit-servingUnit" value={formData.servingUnit} onChange={(e) => setFormData({ ...formData, servingUnit: e.target.value })} />
            </div>
            <div className="field">
              <label htmlFor="edit-caloriesKcal">Calories (kcal)</label>
              <InputText id="edit-caloriesKcal" type="number" value={String(formData.caloriesKcal)} onChange={(e) => setFormData({ ...formData, caloriesKcal: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="edit-proteinG">Protein (g)</label>
              <InputText id="edit-proteinG" type="number" value={String(formData.proteinG)} onChange={(e) => setFormData({ ...formData, proteinG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="edit-carbsG">Carbs (g)</label>
              <InputText id="edit-carbsG" type="number" value={String(formData.carbsG)} onChange={(e) => setFormData({ ...formData, carbsG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
            <div className="field">
              <label htmlFor="edit-fatG">Fat (g)</label>
              <InputText id="edit-fatG" type="number" value={String(formData.fatG)} onChange={(e) => setFormData({ ...formData, fatG: Number.parseFloat(e.target.value) || 0 })} />
            </div>
          </div>
        </Dialog>
      </div>
    </AdminLayout>
  );
}