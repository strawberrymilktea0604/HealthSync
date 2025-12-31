import { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import AdminLayout from "@/components/admin/AdminLayout";
import { Card } from 'primereact/card';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { FileUpload } from 'primereact/fileupload';
import { exerciseService, Exercise, UpdateExerciseDto } from '@/services/exerciseService';
import { toast } from 'sonner';
import 'primeflex/primeflex.css';

export default function ExerciseManagement() {
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [showImageDialog, setShowImageDialog] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);
  const navigate = useNavigate();
  const fileUploadRef = useRef<FileUpload>(null);

  const [formData, setFormData] = useState({
    name: '',
    muscleGroup: '',
    difficulty: '',
    equipment: '',
    description: ''
  });

  const muscleGroups = [
    { label: 'Ngực', value: 'Chest' },
    { label: 'Lưng', value: 'Back' },
    { label: 'Chân', value: 'Legs' },
    { label: 'Vai', value: 'Shoulders' },
    { label: 'Tay', value: 'Arms' },
    { label: 'Bụng', value: 'Abs' }
  ];

  const difficulties = [
    { label: 'Dễ', value: 'Beginner' },
    { label: 'Trung bình', value: 'Intermediate' },
    { label: 'Khó', value: 'Advanced' }
  ];

  useEffect(() => {
    fetchExercises();
  }, []);

  const fetchExercises = async () => {
    try {
      setLoading(true);
      const data = await exerciseService.getExercises();
      setExercises(data);
    } catch (error: unknown) {
      console.error('Error fetching exercises:', error);
      toast.error('Không thể tải danh sách bài tập');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    try {
      await exerciseService.createExercise(formData);
      toast.success('Tạo bài tập thành công');
      setShowCreateDialog(false);
      setFormData({ name: '', muscleGroup: '', difficulty: '', equipment: '', description: '' });
      fetchExercises();
    } catch (error) {
      toast.error('Không thể tạo bài tập');
    }
  };

  const handleEdit = async () => {
    if (!selectedExercise) return;
    try {
      const updateData: UpdateExerciseDto = {
        exerciseId: selectedExercise.exerciseId,
        ...formData
      };
      await exerciseService.updateExercise(selectedExercise.exerciseId, updateData);
      toast.success('Cập nhật bài tập thành công');
      setShowEditDialog(false);
      setSelectedExercise(null);
      fetchExercises();
    } catch (error) {
      toast.error('Không thể cập nhật bài tập');
    }
  };

  const handleDelete = async (exercise: Exercise) => {
    if (!confirm('Bạn có chắc muốn xóa bài tập này?')) return;
    try {
      await exerciseService.deleteExercise(exercise.exerciseId);
      toast.success('Xóa bài tập thành công');
      fetchExercises();
    } catch (error) {
      toast.error('Không thể xóa bài tập');
    }
  };

  const openEditDialog = (exercise: Exercise) => {
    setSelectedExercise(exercise);
    setFormData({
      name: exercise.name,
      muscleGroup: exercise.muscleGroup,
      difficulty: exercise.difficulty,
      equipment: exercise.equipment,
      description: exercise.description
    });
    setShowEditDialog(true);
  };

  const filteredExercises = exercises.filter(exercise =>
    exercise.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exercise.muscleGroup.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const imageTemplate = (rowData: Exercise) => {
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
    if (!selectedExercise || event.files.length === 0) return;

    try {
      setUploadingImage(true);
      const file = event.files[0];
      await exerciseService.uploadExerciseImage(selectedExercise.exerciseId, file);
      toast.success('Upload ảnh thành công');
      setShowImageDialog(false);
      setSelectedExercise(null);
      await fetchExercises();
      if (fileUploadRef.current) {
        fileUploadRef.current.clear();
      }
    } catch (error) {
      toast.error('Không thể upload ảnh');
    } finally {
      setUploadingImage(false);
    }
  };

  const openImageDialog = (exercise: Exercise) => {
    setSelectedExercise(exercise);
    setShowImageDialog(true);
  };

  const actionTemplate = (rowData: Exercise) => (
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

  return (
    <AdminLayout>
      <div className="p-4">
        <Card title="Quản lý Bài tập">
          <div className="flex justify-content-between align-items-center mb-4">
            <div className="flex gap-2">
              <InputText
                placeholder="Tìm kiếm bài tập..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-12rem"
              />
            </div>
            <Button
              label="Thêm bài tập"
              icon="pi pi-plus"
              onClick={() => setShowCreateDialog(true)}
              className="p-button-primary"
            />
          </div>

          <DataTable
            value={filteredExercises}
            loading={loading}
            paginator
            rows={10}
            rowsPerPageOptions={[5, 10, 25]}
            emptyMessage="Không có bài tập nào"
          >
            <Column field="name" header="Tên bài tập" sortable />
            <Column header="Ảnh" body={imageTemplate} style={{ width: '100px' }} />
            <Column field="muscleGroup" header="Nhóm cơ" sortable />
            <Column field="difficulty" header="Độ khó" sortable />
            <Column field="equipment" header="Dụng cụ" sortable />
            <Column field="description" header="Mô tả" style={{ maxWidth: '200px' }} />
            <Column header="Thao tác" body={actionTemplate} style={{ width: '150px' }} />
          </DataTable>
        </Card>

        {/* Create Dialog */}
        <Dialog
          header="Thêm bài tập mới"
          visible={showCreateDialog}
          onHide={() => setShowCreateDialog(false)}
          footer={
            <div>
              <Button label="Hủy" icon="pi pi-times" onClick={() => setShowCreateDialog(false)} className="p-button-text" />
              <Button label="Tạo" icon="pi pi-check" onClick={handleCreate} />
            </div>
          }
        >
          <div className="p-fluid">
            <div className="field">
              <label htmlFor="name">Tên bài tập</label>
              <InputText id="name" value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} />
            </div>
            <div className="field">
              <label htmlFor="muscleGroup">Nhóm cơ</label>
              <Dropdown id="muscleGroup" value={formData.muscleGroup} options={muscleGroups} onChange={(e) => setFormData({...formData, muscleGroup: e.value})} placeholder="Chọn nhóm cơ" />
            </div>
            <div className="field">
              <label htmlFor="difficulty">Độ khó</label>
              <Dropdown id="difficulty" value={formData.difficulty} options={difficulties} onChange={(e) => setFormData({...formData, difficulty: e.value})} placeholder="Chọn độ khó" />
            </div>
            <div className="field">
              <label htmlFor="equipment">Dụng cụ</label>
              <InputText id="equipment" value={formData.equipment} onChange={(e) => setFormData({...formData, equipment: e.target.value})} />
            </div>
            <div className="field">
              <label htmlFor="description">Mô tả</label>
              <InputTextarea id="description" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})} rows={4} />
            </div>
          </div>
        </Dialog>

        {/* Edit Dialog */}
        <Dialog
          header="Chỉnh sửa bài tập"
          visible={showEditDialog}
          onHide={() => setShowEditDialog(false)}
          footer={
            <div>
              <Button label="Hủy" icon="pi pi-times" onClick={() => setShowEditDialog(false)} className="p-button-text" />
              <Button label="Lưu" icon="pi pi-check" onClick={handleEdit} />
            </div>
          }
        >
          <div className="p-fluid">
            <div className="field">
              <label htmlFor="edit-name">Tên bài tập</label>
              <InputText id="edit-name" value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} />
            </div>
            <div className="field">
              <label htmlFor="edit-muscleGroup">Nhóm cơ</label>
              <Dropdown id="edit-muscleGroup" value={formData.muscleGroup} options={muscleGroups} onChange={(e) => setFormData({...formData, muscleGroup: e.value})} placeholder="Chọn nhóm cơ" />
            </div>
            <div className="field">
              <label htmlFor="edit-difficulty">Độ khó</label>
              <Dropdown id="edit-difficulty" value={formData.difficulty} options={difficulties} onChange={(e) => setFormData({...formData, difficulty: e.value})} placeholder="Chọn độ khó" />
            </div>
            <div className="field">
              <label htmlFor="edit-equipment">Dụng cụ</label>
              <InputText id="edit-equipment" value={formData.equipment} onChange={(e) => setFormData({...formData, equipment: e.target.value})} />
            </div>
            <div className="field">
              <label htmlFor="edit-description">Mô tả</label>
              <InputTextarea id="edit-description" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})} rows={4} />
            </div>
          </div>
        </Dialog>

        {/* Image Upload Dialog */}
        <Dialog
          header="Upload ảnh bài tập"
          visible={showImageDialog}
          onHide={() => {
            setShowImageDialog(false);
            setSelectedExercise(null);
            if (fileUploadRef.current) {
              fileUploadRef.current.clear();
            }
          }}
          style={{ width: '450px' }}
        >
          <div className="p-fluid">
            {selectedExercise?.imageUrl && (
              <div className="field">
                <label>Ảnh hiện tại</label>
                <img 
                  src={`${selectedExercise.imageUrl}?t=${Date.now()}`}
                  alt={selectedExercise.name}
                  className="w-full border-round mb-3"
                />
              </div>
            )}
            <div className="field">
              <label>Chọn ảnh mới</label>
              <FileUpload
                ref={fileUploadRef}
                mode="basic"
                name="file"
                accept="image/*"
                maxFileSize={5000000}
                customUpload
                uploadHandler={handleImageUpload}
                auto
                chooseLabel="Chọn ảnh"
                disabled={uploadingImage}
              />
              <small className="text-400">Kích thước tối đa: 5MB. Định dạng: JPG, PNG</small>
            </div>
          </div>
        </Dialog>
      </div>
    </AdminLayout>
  );
}