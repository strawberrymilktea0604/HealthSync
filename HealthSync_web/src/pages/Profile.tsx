import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { Loader2, ArrowLeft, Camera, User, Calendar, Ruler, Activity, Weight } from "lucide-react";
import { format } from "date-fns";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { useAuth } from "@/contexts/AuthContext";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import authService, { UpdateProfileRequest } from "../services/authService";

export default function Profile() {
    const [fullName, setFullName] = useState("");
    const [dateOfBirth, setDateOfBirth] = useState("");
    const [gender, setGender] = useState("");
    const [heightCm, setHeightCm] = useState("");
    const [weightKg, setWeightKg] = useState("");
    const [activityLevel, setActivityLevel] = useState("Moderate");
    const [avatarUrl, setAvatarUrl] = useState<string | undefined>(undefined);
    const [isLoading, setIsLoading] = useState(false);
    const [isFetching, setIsFetching] = useState(true);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const navigate = useNavigate();
    const { toast } = useToast();
    const { user, setUser } = useAuth();

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const profile = await authService.getProfile();
                setFullName(profile.fullName);
                // Assuming dob comes as DateTime string, format to YYYY-MM-DD for input
                if (profile.dob) {
                    const date = new Date(profile.dob);
                    setDateOfBirth(format(date, 'yyyy-MM-dd'));
                }
                setGender(profile.gender);
                setHeightCm(profile.heightCm.toString());
                setWeightKg(profile.weightKg.toString());
                setActivityLevel(profile.activityLevel);
                setAvatarUrl(profile.avatarUrl);
            } catch (error) {
                console.error("Error fetching profile:", error);
                toast({
                    title: "Error",
                    description: "Failed to load profile data.",
                    variant: "destructive",
                });
            } finally {
                setIsFetching(false);
            }
        };

        if (user) {
            fetchProfile();
        } else {
            navigate("/login");
        }
    }, [user, navigate, toast]);

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        // Validate size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            toast({
                title: "Error",
                description: "Image size must be less than 5MB.",
                variant: "destructive",
            });
            return;
        }

        // Validate type
        if (!file.type.startsWith("image/")) {
            toast({
                title: "Error",
                description: "Please upload an image file.",
                variant: "destructive",
            });
            return;
        }

        try {
            setIsLoading(true);
            const result = await authService.uploadAvatar(file);
            setAvatarUrl(result.avatarUrl);

            // Update local user context if avatar is stored there
            if (user) {
                setUser({ ...user, avatar: result.avatarUrl }); // Note: AuthResponse/UserContext might need 'avatar' property update if it differs
            }

            toast({
                title: "Success",
                description: "Avatar updated successfully!",
            });
        } catch (error) {
            console.error("Error uploading avatar:", error);
            toast({
                title: "Error",
                description: "Failed to upload avatar.",
                variant: "destructive",
            });
        } finally {
            setIsLoading(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!fullName.trim()) return toast({ title: "Error", description: "Full Name is required", variant: "destructive" });
        if (!dateOfBirth) return toast({ title: "Error", description: "Date of Birth is required", variant: "destructive" });
        if (!gender) return toast({ title: "Error", description: "Gender is required", variant: "destructive" });

        const height = parseFloat(heightCm);
        const weight = parseFloat(weightKg);

        if (!heightCm || height <= 0 || height > 300) return toast({ title: "Error", description: "Invalid height", variant: "destructive" });
        if (!weightKg || weight <= 0 || weight > 500) return toast({ title: "Error", description: "Invalid weight", variant: "destructive" });

        setIsLoading(true);
        try {
            const updateData: UpdateProfileRequest = {
                fullName: fullName.trim(),
                dob: dateOfBirth,
                gender,
                heightCm: height,
                weightKg: weight,
                activityLevel,
                avatarUrl: avatarUrl
            };

            await authService.updateProfile(updateData);

            // Update user context
            if (user) {
                setUser({ ...user, fullName: fullName.trim() });
            }

            toast({
                title: "Success",
                description: "Profile updated successfully!",
            });
        } catch (error) {
            console.error('Error updating profile:', error);
            toast({
                title: "Error",
                description: "Failed to update profile.",
                variant: "destructive",
            });
        } finally {
            setIsLoading(false);
        }
    };

    if (isFetching) {
        return (
            <div className="flex items-center justify-center min-h-screen bg-[#FDFBD4]">
                <Loader2 className="w-12 h-12 animate-spin text-black" />
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-[#FDFBD4] p-4 md:p-8">
            <div className="max-w-4xl mx-auto bg-white/80 backdrop-blur-sm rounded-3xl p-6 md:p-10 shadow-xl border border-white/50">

                {/* Header */}
                <div className="flex items-center justify-between mb-8">
                    <Button variant="ghost" className="gap-2" onClick={() => navigate("/dashboard")}>
                        <ArrowLeft className="w-5 h-5" />
                        Back to Dashboard
                    </Button>
                    <h1 className="text-3xl font-bold text-center flex-1 pr-24">Profile Settings</h1>
                </div>

                <div className="grid md:grid-cols-[300px_1fr] gap-12">

                    {/* Left Column: Avatar */}
                    <div className="flex flex-col items-center space-y-4">
                        <div className="relative group">
                            <Avatar className="w-48 h-48 border-4 border-white shadow-lg">
                                <AvatarImage src={avatarUrl} className="object-cover" />
                                <AvatarFallback className="text-4xl bg-gray-200">{fullName.charAt(0)}</AvatarFallback>
                            </Avatar>
                            <div
                                className="absolute inset-0 bg-black/40 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                                onClick={() => fileInputRef.current?.click()}
                            >
                                <Camera className="w-10 h-10 text-white" />
                            </div>
                            <Input
                                type="file"
                                ref={fileInputRef}
                                className="hidden"
                                accept="image/*"
                                onChange={handleFileChange}
                            />
                        </div>
                        <p className="text-sm text-gray-500 italic">Click image to change avatar</p>
                    </div>

                    {/* Right Column: Form */}
                    <form onSubmit={handleSubmit} className="space-y-6">

                        <div className="space-y-2">
                            <Label htmlFor="fullName" className="text-lg flex items-center gap-2">
                                <User className="w-5 h-5" /> Full Name
                            </Label>
                            <Input
                                id="fullName"
                                value={fullName}
                                onChange={(e) => setFullName(e.target.value)}
                                className="bg-white/50 border-gray-300 text-lg py-6"
                            />
                        </div>

                        <div className="grid grid-cols-2 gap-6">
                            <div className="space-y-2">
                                <Label htmlFor="dob" className="text-lg flex items-center gap-2">
                                    <Calendar className="w-5 h-5" /> Date of Birth
                                </Label>
                                <Input
                                    id="dob"
                                    type="date"
                                    max={new Date().toISOString().split("T")[0]}
                                    value={dateOfBirth}
                                    onChange={(e) => setDateOfBirth(e.target.value)}
                                    className="bg-white/50 border-gray-300 text-lg py-6"
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="gender" className="text-lg flex items-center gap-2">
                                    Gender
                                </Label>
                                <Select value={gender} onValueChange={setGender}>
                                    <SelectTrigger className="bg-white/50 border-gray-300 text-lg py-6 h-auto">
                                        <SelectValue placeholder="Select Gender" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="Male">Male</SelectItem>
                                        <SelectItem value="Female">Female</SelectItem>
                                        <SelectItem value="Other">Other</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-6">
                            <div className="space-y-2">
                                <Label htmlFor="height" className="text-lg flex items-center gap-2">
                                    <Ruler className="w-5 h-5" /> Height (cm)
                                </Label>
                                <Input
                                    id="height"
                                    type="number"
                                    value={heightCm}
                                    onChange={(e) => setHeightCm(e.target.value)}
                                    className="bg-white/50 border-gray-300 text-lg py-6"
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="weight" className="text-lg flex items-center gap-2">
                                    <Weight className="w-5 h-5" /> Weight (kg)
                                </Label>
                                <Input
                                    id="weight"
                                    type="number"
                                    value={weightKg}
                                    onChange={(e) => setWeightKg(e.target.value)}
                                    className="bg-white/50 border-gray-300 text-lg py-6"
                                />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="activity" className="text-lg flex items-center gap-2">
                                <Activity className="w-5 h-5" /> Activity Level
                            </Label>
                            <Select value={activityLevel} onValueChange={setActivityLevel}>
                                <SelectTrigger className="bg-white/50 border-gray-300 text-lg py-6 h-auto">
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="Sedentary">Sedentary (Little to no exercise)</SelectItem>
                                    <SelectItem value="Light">Light (Light exercise 1-3 days/week)</SelectItem>
                                    <SelectItem value="Moderate">Moderate (Moderate exercise 3-5 days/week)</SelectItem>
                                    <SelectItem value="Active">Active (Heavy exercise 6-7 days/week)</SelectItem>
                                    <SelectItem value="VeryActive">Very Active (Very heavy exercise)</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <motion.div
                            className="pt-4"
                            whileHover={{ scale: 1.02 }}
                            whileTap={{ scale: 0.98 }}
                        >
                            <Button
                                type="submit"
                                disabled={isLoading}
                                className="w-full text-xl py-6 rounded-xl bg-black hover:bg-black/90 text-[#FDFBD4]"
                            >
                                {isLoading ? <Loader2 className="w-6 h-6 animate-spin mr-2" /> : null}
                                Save Changes
                            </Button>
                        </motion.div>

                    </form>
                </div>
            </div>
        </div>
    );
}
