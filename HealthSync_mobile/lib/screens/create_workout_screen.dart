import 'package:flutter/material.dart';
import '../models/workout_model.dart';
import '../services/workout_service.dart';

class CreateWorkoutScreen extends StatefulWidget {
  const CreateWorkoutScreen({super.key});

  @override
  State<CreateWorkoutScreen> createState() => _CreateWorkoutScreenState();
}

class _CreateWorkoutScreenState extends State<CreateWorkoutScreen> {
  final WorkoutService _workoutService = WorkoutService();
  final _formKey = GlobalKey<FormState>();
  
  DateTime _workoutDate = DateTime.now();
  int _durationMin = 60;
  String _notes = '';
  List<Exercise> _exercises = [];
  List<Exercise> _filteredExercises = [];
  final List<_ExerciseSessionData> _selectedExercises = [];
  
  String _searchTerm = '';
  String? _muscleGroupFilter;
  String? _difficultyFilter;
  
  bool _isLoading = false;
  bool _isLoadingExercises = true;

  final List<String> _muscleGroups = [
    'Chest',
    'Back',
    'Shoulders',
    'Arms',
    'Legs',
    'Core',
    'Full Body'
  ];

  final List<String> _difficulties = ['Beginner', 'Intermediate', 'Advanced'];

  @override
  void initState() {
    super.initState();
    _loadExercises();
  }

  Future<void> _loadExercises() async {
    try {
      setState(() => _isLoadingExercises = true);
      final exercises = await _workoutService.getExercises();
      setState(() {
        _exercises = exercises;
        _filteredExercises = exercises;
        _isLoadingExercises = false;
      });
    } catch (e) {
      setState(() => _isLoadingExercises = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Không thể tải bài tập: $e')),
        );
      }
    }
  }

  void _filterExercises() {
    setState(() {
      _filteredExercises = _exercises.where((ex) {
        bool matchesSearch = _searchTerm.isEmpty ||
            ex.name.toLowerCase().contains(_searchTerm.toLowerCase()) ||
            (ex.description?.toLowerCase().contains(_searchTerm.toLowerCase()) ?? false);
        
        bool matchesMuscleGroup = _muscleGroupFilter == null ||
            ex.muscleGroup == _muscleGroupFilter;
        
        bool matchesDifficulty = _difficultyFilter == null ||
            ex.difficulty == _difficultyFilter;

        return matchesSearch && matchesMuscleGroup && matchesDifficulty;
      }).toList();
    });
  }

  void _addExercise(Exercise exercise) {
    if (_selectedExercises.any((e) => e.exercise.exerciseId == exercise.exerciseId)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Bài tập này đã được thêm')),
      );
      return;
    }

    setState(() {
      _selectedExercises.add(_ExerciseSessionData(
        exercise: exercise,
        sets: 3,
        reps: 10,
        weightKg: 0,
        restSec: 60,
      ));
    });

    Navigator.pop(context);
  }

  void _removeExercise(int index) {
    setState(() {
      _selectedExercises.removeAt(index);
    });
  }

  Future<void> _selectDate() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _workoutDate,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
    );
    if (picked != null) {
      setState(() => _workoutDate = picked);
    }
  }

  Future<void> _saveWorkout() async {
    if (_selectedExercises.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng thêm ít nhất một bài tập')),
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      final workoutLog = CreateWorkoutLog(
        workoutDate: _workoutDate,
        durationMin: _durationMin,
        notes: _notes.isEmpty ? null : _notes,
        exerciseSessions: _selectedExercises
            .map((e) => ExerciseSession(
                  exerciseId: e.exercise.exerciseId,
                  sets: e.sets,
                  reps: e.reps,
                  weightKg: e.weightKg,
                  restSec: e.restSec > 0 ? e.restSec : null,
                ))
            .toList(),
      );

      await _workoutService.createWorkoutLog(workoutLog);
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Đã lưu nhật ký luyện tập!')),
        );
        Navigator.pop(context, true);
      }
    } catch (e) {
      setState(() => _isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F1E8),
      appBar: AppBar(
        backgroundColor: const Color(0xFFF5F1E8),
        elevation: 0,
        title: const Text(
          'Buổi Tập Mới',
          style: TextStyle(
            color: Colors.black,
            fontSize: 24,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: Form(
        key: _formKey,
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildWorkoutDetailsCard(),
              const SizedBox(height: 16),
              _buildSelectedExercisesCard(),
              const SizedBox(height: 16),
              _buildSaveButton(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildWorkoutDetailsCard() {
    return Card(
      color: const Color(0xFFE8DCC4),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Thông tin buổi tập',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            ListTile(
              contentPadding: EdgeInsets.zero,
              leading: const Icon(Icons.calendar_today),
              title: const Text('Ngày tập'),
              subtitle: Text(
                '${_workoutDate.day}/${_workoutDate.month}/${_workoutDate.year}',
              ),
              onTap: _selectDate,
            ),
            const Divider(),
            ListTile(
              contentPadding: EdgeInsets.zero,
              leading: const Icon(Icons.schedule),
              title: const Text('Thời gian (phút)'),
              subtitle: Slider(
                value: _durationMin.toDouble(),
                min: 15,
                max: 180,
                divisions: 33,
                label: '$_durationMin phút',
                onChanged: (value) {
                  setState(() => _durationMin = value.toInt());
                },
              ),
            ),
            const Divider(),
            TextField(
              decoration: const InputDecoration(
                labelText: 'Ghi chú',
                hintText: 'Cảm nhận về buổi tập...',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
              onChanged: (value) => _notes = value,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSelectedExercisesCard() {
    return Card(
      color: const Color(0xFFE8DCC4),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Các bài tập (${_selectedExercises.length})',
                  style: const TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                ElevatedButton.icon(
                  onPressed: _showExerciseLibrary,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF2D3E2E),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(20),
                    ),
                  ),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Thêm'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            if (_selectedExercises.isEmpty)
              const Center(
                child: Padding(
                  padding: EdgeInsets.all(32),
                  child: Text(
                    'Chưa có bài tập nào.\nNhấn "Thêm" để chọn bài tập.',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: Colors.grey),
                  ),
                ),
              )
            else
              ..._selectedExercises.asMap().entries.map((entry) {
                final index = entry.key;
                final session = entry.value;
                return _buildExerciseSessionCard(index, session);
              }),
          ],
        ),
      ),
    );
  }

  Widget _buildExerciseSessionCard(int index, _ExerciseSessionData session) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      color: const Color(0xFFF5F1E8),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        session.exercise.name,
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                        ),
                      ),
                      Text(
                        session.exercise.muscleGroup,
                        style: TextStyle(
                          color: Colors.grey[600],
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.delete, color: Colors.red),
                  onPressed: () => _removeExercise(index),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Sets', style: TextStyle(fontSize: 12)),
                      TextField(
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          isDense: true,
                          border: OutlineInputBorder(),
                        ),
                        controller: TextEditingController(
                          text: session.sets.toString(),
                        ),
                        onChanged: (value) {
                          setState(() {
                            session.sets = int.tryParse(value) ?? 3;
                          });
                        },
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Reps', style: TextStyle(fontSize: 12)),
                      TextField(
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          isDense: true,
                          border: OutlineInputBorder(),
                        ),
                        controller: TextEditingController(
                          text: session.reps.toString(),
                        ),
                        onChanged: (value) {
                          setState(() {
                            session.reps = int.tryParse(value) ?? 10;
                          });
                        },
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Tạ (kg)', style: TextStyle(fontSize: 12)),
                      TextField(
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        decoration: const InputDecoration(
                          isDense: true,
                          border: OutlineInputBorder(),
                        ),
                        controller: TextEditingController(
                          text: session.weightKg.toString(),
                        ),
                        onChanged: (value) {
                          setState(() {
                            session.weightKg = double.tryParse(value) ?? 0;
                          });
                        },
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _showExerciseLibrary() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: const Color(0xFFF5F1E8),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.9,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, scrollController) {
          return StatefulBuilder(
            builder: (context, setModalState) {
              return Column(
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      children: [
                        Container(
                          width: 40,
                          height: 4,
                          decoration: BoxDecoration(
                            color: Colors.grey[300],
                            borderRadius: BorderRadius.circular(2),
                          ),
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          'Thư viện Bài tập',
                          style: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 16),
                        TextField(
                          decoration: InputDecoration(
                            hintText: 'Tìm kiếm bài tập...',
                            prefixIcon: const Icon(Icons.search),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                            filled: true,
                            fillColor: Colors.white,
                          ),
                          onChanged: (value) {
                            setModalState(() {
                              _searchTerm = value;
                              _filterExercises();
                            });
                          },
                        ),
                        const SizedBox(height: 12),
                        Row(
                          children: [
                            Expanded(
                              child: DropdownButtonFormField<String>(
                                decoration: InputDecoration(
                                  labelText: 'Nhóm cơ',
                                  border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  filled: true,
                                  fillColor: Colors.white,
                                ),
                                initialValue: _muscleGroupFilter,
                                items: [
                                  const DropdownMenuItem(
                                    value: null,
                                    child: Text('Tất cả'),
                                  ),
                                  ..._muscleGroups.map((group) =>
                                      DropdownMenuItem(
                                        value: group,
                                        child: Text(group),
                                      ))
                                ],
                                onChanged: (value) {
                                  setModalState(() {
                                    _muscleGroupFilter = value;
                                    _filterExercises();
                                  });
                                },
                              ),
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: DropdownButtonFormField<String>(
                                decoration: InputDecoration(
                                  labelText: 'Độ khó',
                                  border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  filled: true,
                                  fillColor: Colors.white,
                                ),
                                initialValue: _difficultyFilter,
                                items: [
                                  const DropdownMenuItem(
                                    value: null,
                                    child: Text('Tất cả'),
                                  ),
                                  ..._difficulties.map((diff) =>
                                      DropdownMenuItem(
                                        value: diff,
                                        child: Text(diff),
                                      ))
                                ],
                                onChanged: (value) {
                                  setModalState(() {
                                    _difficultyFilter = value;
                                    _filterExercises();
                                  });
                                },
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                  Expanded(
                    child: _isLoadingExercises
                        ? const Center(child: CircularProgressIndicator())
                        : ListView.builder(
                            controller: scrollController,
                            padding: const EdgeInsets.all(16),
                            itemCount: _filteredExercises.length,
                            itemBuilder: (context, index) {
                              final exercise = _filteredExercises[index];
                              return Card(
                                margin: const EdgeInsets.only(bottom: 12),
                                color: const Color(0xFFE8DCC4),
                                child: ListTile(
                                  title: Text(
                                    exercise.name,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                  subtitle: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      const SizedBox(height: 4),
                                      Row(
                                        children: [
                                          Container(
                                            padding: const EdgeInsets.symmetric(
                                              horizontal: 8,
                                              vertical: 4,
                                            ),
                                            decoration: BoxDecoration(
                                              color: const Color(0xFF2D3E2E)
                                                  .withValues(alpha: 0.1),
                                              borderRadius:
                                                  BorderRadius.circular(8),
                                            ),
                                            child: Text(
                                              exercise.muscleGroup,
                                              style: const TextStyle(
                                                fontSize: 12,
                                              ),
                                            ),
                                          ),
                                          const SizedBox(width: 8),
                                          Container(
                                            padding: const EdgeInsets.symmetric(
                                              horizontal: 8,
                                              vertical: 4,
                                            ),
                                            decoration: BoxDecoration(
                                              color: Colors.grey[300],
                                              borderRadius:
                                                  BorderRadius.circular(8),
                                            ),
                                            child: Text(
                                              exercise.difficulty,
                                              style: const TextStyle(
                                                fontSize: 12,
                                              ),
                                            ),
                                          ),
                                        ],
                                      ),
                                      if (exercise.description != null) ...[
                                        const SizedBox(height: 4),
                                        Text(
                                          exercise.description!,
                                          maxLines: 2,
                                          overflow: TextOverflow.ellipsis,
                                          style: TextStyle(
                                            fontSize: 12,
                                            color: Colors.grey[600],
                                          ),
                                        ),
                                      ],
                                    ],
                                  ),
                                  trailing: IconButton(
                                    icon: const Icon(Icons.add_circle),
                                    color: const Color(0xFF2D3E2E),
                                    onPressed: () => _addExercise(exercise),
                                  ),
                                ),
                              );
                            },
                          ),
                  ),
                ],
              );
            },
          );
        },
      ),
    );
  }

  Widget _buildSaveButton() {
    return SizedBox(
      width: double.infinity,
      height: 50,
      child: ElevatedButton(
        onPressed: _isLoading ? null : _saveWorkout,
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color(0xFF2D3E2E),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(25),
          ),
        ),
        child: _isLoading
            ? const CircularProgressIndicator(color: Colors.white)
            : const Text(
                'Lưu Buổi Tập',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
      ),
    );
  }
}

class _ExerciseSessionData {
  final Exercise exercise;
  int sets;
  int reps;
  double weightKg;
  int restSec;

  _ExerciseSessionData({
    required this.exercise,
    required this.sets,
    required this.reps,
    required this.weightKg,
    required this.restSec,
  });
}
