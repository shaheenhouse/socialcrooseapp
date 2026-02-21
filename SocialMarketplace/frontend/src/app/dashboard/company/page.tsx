"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  Building2,
  Plus,
  Pencil,
  Eye,
  Users,
  MapPin,
  Mail,
  Phone,
  Globe,
  Calendar,
  Briefcase,
  Trash2,
  UserPlus,
  ChevronRight,
  Loader2,
  Search,
  ArrowLeft,
  X,
} from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useAuthStore } from "@/store/auth-store";
import { companyApi } from "@/lib/api";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.08 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.4 } },
};

const INDUSTRIES = [
  "Technology",
  "Healthcare",
  "Finance",
  "Education",
  "E-Commerce",
  "Manufacturing",
  "Real Estate",
  "Marketing & Advertising",
  "Consulting",
  "Media & Entertainment",
  "Food & Beverage",
  "Transportation",
  "Agriculture",
  "Construction",
  "Retail",
  "Telecommunications",
  "Energy",
  "Fashion & Apparel",
  "Tourism & Hospitality",
  "Legal Services",
  "Other",
];

const COMPANY_TYPES = [
  "Startup",
  "SME",
  "Enterprise",
  "Agency",
  "Freelance",
];

const COMPANY_SIZES = [
  "1-10",
  "11-50",
  "51-200",
  "201-500",
  "500+",
];

interface CompanyFormData {
  name: string;
  legalName: string;
  description: string;
  industry: string;
  companyType: string;
  companySize: string;
  email: string;
  phone: string;
  website: string;
  address: string;
  city: string;
  country: string;
  foundedYear: string;
  logoUrl: string;
}

const emptyForm: CompanyFormData = {
  name: "",
  legalName: "",
  description: "",
  industry: "",
  companyType: "",
  companySize: "",
  email: "",
  phone: "",
  website: "",
  address: "",
  city: "",
  country: "",
  foundedYear: "",
  logoUrl: "",
};

interface Employee {
  id?: string;
  userId: string;
  userName?: string;
  fullName?: string;
  avatarUrl?: string;
  title: string;
  department?: string;
}

type ViewMode = "grid" | "detail";

export default function CompanyPage() {
  const { user } = useAuthStore();
  const queryClient = useQueryClient();

  const [viewMode, setViewMode] = useState<ViewMode>("grid");
  const [selectedCompanyId, setSelectedCompanyId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [addEmployeeOpen, setAddEmployeeOpen] = useState(false);
  const [formData, setFormData] = useState<CompanyFormData>(emptyForm);
  const [employeeForm, setEmployeeForm] = useState({ userId: "", title: "", department: "" });

  const { data: companiesData, isLoading } = useQuery({
    queryKey: ["my-companies"],
    queryFn: () => companyApi.getMy(),
    enabled: !!user,
  });

  const companies: any[] = companiesData?.data?.items ?? companiesData?.data ?? [];

  const { data: companyDetailData, isLoading: detailLoading } = useQuery({
    queryKey: ["company-detail", selectedCompanyId],
    queryFn: () => companyApi.getById(selectedCompanyId!),
    enabled: !!selectedCompanyId,
  });

  const companyDetail: any = companyDetailData?.data ?? null;

  const { data: employeesData, isLoading: employeesLoading } = useQuery({
    queryKey: ["company-employees", selectedCompanyId],
    queryFn: () => companyApi.getEmployees(selectedCompanyId!),
    enabled: !!selectedCompanyId && viewMode === "detail",
  });

  const employees: Employee[] = employeesData?.data?.items ?? employeesData?.data ?? [];

  const createMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => companyApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-companies"] });
      setCreateOpen(false);
      setFormData(emptyForm);
      toast.success("Company created successfully!");
    },
    onError: () => toast.error("Failed to create company"),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) =>
      companyApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-companies"] });
      queryClient.invalidateQueries({ queryKey: ["company-detail", selectedCompanyId] });
      setEditOpen(false);
      toast.success("Company updated successfully!");
    },
    onError: () => toast.error("Failed to update company"),
  });

  const addEmployeeMutation = useMutation({
    mutationFn: ({ companyId, data }: { companyId: string; data: { userId: string; title: string; department?: string } }) =>
      companyApi.addEmployee(companyId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["company-employees", selectedCompanyId] });
      setAddEmployeeOpen(false);
      setEmployeeForm({ userId: "", title: "", department: "" });
      toast.success("Employee added successfully!");
    },
    onError: () => toast.error("Failed to add employee"),
  });

  const removeEmployeeMutation = useMutation({
    mutationFn: ({ companyId, userId }: { companyId: string; userId: string }) =>
      companyApi.removeEmployee(companyId, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["company-employees", selectedCompanyId] });
      toast.success("Employee removed");
    },
    onError: () => toast.error("Failed to remove employee"),
  });

  const handleCreate = () => {
    if (!formData.name.trim()) {
      toast.error("Company name is required");
      return;
    }
    const payload: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(formData)) {
      if (value) payload[key] = key === "foundedYear" ? parseInt(value) : value;
    }
    createMutation.mutate(payload);
  };

  const handleUpdate = () => {
    if (!selectedCompanyId || !formData.name.trim()) {
      toast.error("Company name is required");
      return;
    }
    const payload: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(formData)) {
      if (value) payload[key] = key === "foundedYear" ? parseInt(value) : value;
    }
    updateMutation.mutate({ id: selectedCompanyId, data: payload });
  };

  const handleAddEmployee = () => {
    if (!selectedCompanyId || !employeeForm.userId.trim() || !employeeForm.title.trim()) {
      toast.error("User ID and Title are required");
      return;
    }
    addEmployeeMutation.mutate({
      companyId: selectedCompanyId,
      data: {
        userId: employeeForm.userId.trim(),
        title: employeeForm.title.trim(),
        department: employeeForm.department.trim() || undefined,
      },
    });
  };

  const openEditDialog = (company: any) => {
    setFormData({
      name: company.name || "",
      legalName: company.legalName || "",
      description: company.description || "",
      industry: company.industry || "",
      companyType: company.companyType || "",
      companySize: company.companySize || "",
      email: company.email || "",
      phone: company.phone || "",
      website: company.website || "",
      address: company.address || "",
      city: company.city || "",
      country: company.country || "",
      foundedYear: company.foundedYear?.toString() || "",
      logoUrl: company.logoUrl || "",
    });
    setSelectedCompanyId(company.id);
    setEditOpen(true);
  };

  const openCompanyDetail = (companyId: string) => {
    setSelectedCompanyId(companyId);
    setViewMode("detail");
  };

  const backToGrid = () => {
    setViewMode("grid");
    setSelectedCompanyId(null);
  };

  const getInitials = (name: string) =>
    name
      .split(" ")
      .map((w) => w[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);

  const sizeLabel = (size: string) => {
    const map: Record<string, string> = {
      "1-10": "1-10 employees",
      "11-50": "11-50 employees",
      "51-200": "51-200 employees",
      "201-500": "201-500 employees",
      "500+": "500+ employees",
    };
    return map[size] || size;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  // ---------- DETAIL VIEW ----------
  if (viewMode === "detail" && selectedCompanyId) {
    const detail = companyDetail;

    return (
      <motion.div
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        transition={{ duration: 0.3 }}
        className="space-y-6"
      >
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={backToGrid}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div className="flex-1">
            <h1 className="text-3xl font-bold">{detail?.name || "Company Details"}</h1>
            <p className="text-muted-foreground">Manage company information and team</p>
          </div>
          <Button variant="outline" onClick={() => detail && openEditDialog(detail)}>
            <Pencil className="mr-2 h-4 w-4" />
            Edit Info
          </Button>
        </div>

        {detailLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : detail ? (
          <div className="grid gap-6 lg:grid-cols-3">
            {/* Company Info */}
            <motion.div variants={itemVariants} className="lg:col-span-2 space-y-6">
              <Card>
                <CardHeader>
                  <div className="flex items-center gap-4">
                    <Avatar className="h-16 w-16">
                      <AvatarImage src={detail.logoUrl} />
                      <AvatarFallback className="text-lg bg-primary/10 text-primary">
                        {getInitials(detail.name || "C")}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <CardTitle className="text-2xl">{detail.name}</CardTitle>
                      {detail.legalName && detail.legalName !== detail.name && (
                        <CardDescription>{detail.legalName}</CardDescription>
                      )}
                      <div className="flex items-center gap-2 mt-2">
                        {detail.industry && <Badge variant="secondary">{detail.industry}</Badge>}
                        {detail.companyType && <Badge variant="outline">{detail.companyType}</Badge>}
                      </div>
                    </div>
                  </div>
                </CardHeader>
                {detail.description && (
                  <CardContent>
                    <p className="text-muted-foreground leading-relaxed">{detail.description}</p>
                  </CardContent>
                )}
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-lg">Company Details</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid gap-4 sm:grid-cols-2">
                    {detail.email && (
                      <InfoRow icon={Mail} label="Email" value={detail.email} />
                    )}
                    {detail.phone && (
                      <InfoRow icon={Phone} label="Phone" value={detail.phone} />
                    )}
                    {detail.website && (
                      <InfoRow icon={Globe} label="Website" value={detail.website} />
                    )}
                    {(detail.city || detail.country) && (
                      <InfoRow
                        icon={MapPin}
                        label="Location"
                        value={[detail.city, detail.country].filter(Boolean).join(", ")}
                      />
                    )}
                    {detail.address && (
                      <InfoRow icon={MapPin} label="Address" value={detail.address} />
                    )}
                    {detail.foundedYear && (
                      <InfoRow icon={Calendar} label="Founded" value={detail.foundedYear.toString()} />
                    )}
                    {detail.companySize && (
                      <InfoRow icon={Users} label="Size" value={sizeLabel(detail.companySize)} />
                    )}
                  </div>
                </CardContent>
              </Card>
            </motion.div>

            {/* Employee Management */}
            <motion.div variants={itemVariants} className="space-y-4">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-3">
                  <div>
                    <CardTitle className="text-lg">Team Members</CardTitle>
                    <CardDescription>{employees.length} member{employees.length !== 1 ? "s" : ""}</CardDescription>
                  </div>
                  <Button size="sm" onClick={() => setAddEmployeeOpen(true)}>
                    <UserPlus className="mr-2 h-4 w-4" />
                    Add
                  </Button>
                </CardHeader>
                <CardContent>
                  {employeesLoading ? (
                    <div className="flex items-center justify-center py-8">
                      <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
                    </div>
                  ) : employees.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-8 text-center">
                      <Users className="h-10 w-10 text-muted-foreground/40 mb-3" />
                      <p className="text-sm text-muted-foreground">No team members yet</p>
                      <p className="text-xs text-muted-foreground mt-1">Add employees to your team</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {employees.map((emp) => (
                        <div
                          key={emp.userId}
                          className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted/50"
                        >
                          <Avatar className="h-9 w-9">
                            <AvatarImage src={emp.avatarUrl} />
                            <AvatarFallback className="text-xs">
                              {getInitials(emp.fullName || emp.userName || "U")}
                            </AvatarFallback>
                          </Avatar>
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium truncate">
                              {emp.fullName || emp.userName || emp.userId}
                            </p>
                            <p className="text-xs text-muted-foreground truncate">
                              {emp.title}
                              {emp.department ? ` - ${emp.department}` : ""}
                            </p>
                          </div>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-muted-foreground hover:text-destructive"
                            onClick={() =>
                              removeEmployeeMutation.mutate({
                                companyId: selectedCompanyId,
                                userId: emp.userId,
                              })
                            }
                            disabled={removeEmployeeMutation.isPending}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            </motion.div>
          </div>
        ) : null}

        {/* Add Employee Dialog */}
        <Dialog open={addEmployeeOpen} onOpenChange={setAddEmployeeOpen}>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>Add Team Member</DialogTitle>
              <DialogDescription>Add an employee to this company</DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-2">
              <div className="space-y-2">
                <label className="text-sm font-medium">User ID *</label>
                <Input
                  placeholder="Enter user ID"
                  value={employeeForm.userId}
                  onChange={(e) => setEmployeeForm((p) => ({ ...p, userId: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Job Title *</label>
                <Input
                  placeholder="e.g. Software Engineer"
                  value={employeeForm.title}
                  onChange={(e) => setEmployeeForm((p) => ({ ...p, title: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Department</label>
                <Input
                  placeholder="e.g. Engineering"
                  value={employeeForm.department}
                  onChange={(e) => setEmployeeForm((p) => ({ ...p, department: e.target.value }))}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setAddEmployeeOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleAddEmployee} disabled={addEmployeeMutation.isPending}>
                {addEmployeeMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Add Member
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Edit Company Dialog */}
        <CompanyFormDialog
          open={editOpen}
          onOpenChange={setEditOpen}
          title="Edit Company"
          description="Update your company information"
          formData={formData}
          setFormData={setFormData}
          onSubmit={handleUpdate}
          isPending={updateMutation.isPending}
          submitLabel="Save Changes"
        />
      </motion.div>
    );
  }

  // ---------- GRID VIEW ----------
  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-6"
    >
      {/* Header */}
      <motion.div variants={itemVariants} className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Companies</h1>
          <p className="text-muted-foreground">
            Manage your businesses and organizations
          </p>
        </div>
        <Button
          onClick={() => {
            setFormData(emptyForm);
            setCreateOpen(true);
          }}
        >
          <Plus className="mr-2 h-4 w-4" />
          Create Company
        </Button>
      </motion.div>

      {/* Company Grid or Empty State */}
      {companies.length === 0 ? (
        <motion.div variants={itemVariants}>
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-16">
              <div className="flex h-20 w-20 items-center justify-center rounded-full bg-primary/10 mb-6">
                <Building2 className="h-10 w-10 text-primary" />
              </div>
              <h2 className="text-2xl font-bold mb-2">Create Your First Company</h2>
              <p className="text-muted-foreground text-center max-w-md mb-6">
                Set up your business profile to manage employees, showcase your company,
                and connect with other professionals on the platform.
              </p>
              <Button
                size="lg"
                onClick={() => {
                  setFormData(emptyForm);
                  setCreateOpen(true);
                }}
              >
                <Plus className="mr-2 h-5 w-5" />
                Create Company
              </Button>
            </CardContent>
          </Card>
        </motion.div>
      ) : (
        <motion.div
          variants={itemVariants}
          className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        >
          <AnimatePresence mode="popLayout">
            {companies.map((company: any) => (
              <motion.div
                key={company.id}
                layout
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.95 }}
                transition={{ duration: 0.3 }}
              >
                <Card className="group relative overflow-hidden transition-all hover:shadow-lg hover:border-primary/30">
                  <CardHeader className="pb-3">
                    <div className="flex items-start gap-3">
                      <Avatar className="h-12 w-12 shrink-0">
                        <AvatarImage src={company.logoUrl} />
                        <AvatarFallback className="bg-primary/10 text-primary font-semibold">
                          {getInitials(company.name || "C")}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <CardTitle className="text-lg truncate">{company.name}</CardTitle>
                        {company.industry && (
                          <Badge variant="secondary" className="mt-1 text-xs">
                            {company.industry}
                          </Badge>
                        )}
                      </div>
                    </div>
                  </CardHeader>

                  <CardContent className="space-y-3 pt-0">
                    {company.description && (
                      <p className="text-sm text-muted-foreground line-clamp-2">
                        {company.description}
                      </p>
                    )}

                    <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
                      {company.companyType && (
                        <span className="flex items-center gap-1">
                          <Briefcase className="h-3 w-3" />
                          {company.companyType}
                        </span>
                      )}
                      {(company.city || company.country) && (
                        <span className="flex items-center gap-1">
                          <MapPin className="h-3 w-3" />
                          {[company.city, company.country].filter(Boolean).join(", ")}
                        </span>
                      )}
                      {company.companySize && (
                        <span className="flex items-center gap-1">
                          <Users className="h-3 w-3" />
                          {company.companySize}
                        </span>
                      )}
                    </div>

                    <Separator />

                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        className="flex-1"
                        onClick={() => openCompanyDetail(company.id)}
                      >
                        <Eye className="mr-1.5 h-3.5 w-3.5" />
                        View
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        className="flex-1"
                        onClick={() => openEditDialog(company)}
                      >
                        <Pencil className="mr-1.5 h-3.5 w-3.5" />
                        Edit
                      </Button>
                      <Button
                        variant="default"
                        size="sm"
                        className="flex-1"
                        onClick={() => openCompanyDetail(company.id)}
                      >
                        <Users className="mr-1.5 h-3.5 w-3.5" />
                        Team
                      </Button>
                    </div>
                  </CardContent>

                  <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-blue-500 to-purple-500 opacity-0 transition-opacity group-hover:opacity-100" />
                </Card>
              </motion.div>
            ))}
          </AnimatePresence>

          {/* Add Company Card */}
          <motion.div layout>
            <Card
              className="flex flex-col items-center justify-center min-h-[280px] border-dashed cursor-pointer transition-all hover:border-primary hover:bg-muted/30"
              onClick={() => {
                setFormData(emptyForm);
                setCreateOpen(true);
              }}
            >
              <div className="flex h-14 w-14 items-center justify-center rounded-full border-2 border-dashed border-muted-foreground/30 mb-4">
                <Plus className="h-6 w-6 text-muted-foreground/50" />
              </div>
              <p className="font-medium text-muted-foreground">Add Company</p>
              <p className="text-xs text-muted-foreground/70 mt-1">Register a new business</p>
            </Card>
          </motion.div>
        </motion.div>
      )}

      {/* Create Company Dialog */}
      <CompanyFormDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
        title="Create Company"
        description="Set up a new business on the platform"
        formData={formData}
        setFormData={setFormData}
        onSubmit={handleCreate}
        isPending={createMutation.isPending}
        submitLabel="Create Company"
      />

      {/* Edit Company Dialog (from grid) */}
      <CompanyFormDialog
        open={editOpen}
        onOpenChange={setEditOpen}
        title="Edit Company"
        description="Update your company information"
        formData={formData}
        setFormData={setFormData}
        onSubmit={handleUpdate}
        isPending={updateMutation.isPending}
        submitLabel="Save Changes"
      />
    </motion.div>
  );
}

// ---------- Shared Components ----------

function InfoRow({ icon: Icon, label, value }: { icon: any; label: string; value: string }) {
  return (
    <div className="flex items-start gap-3">
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-muted">
        <Icon className="h-4 w-4 text-muted-foreground" />
      </div>
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-medium">{value}</p>
      </div>
    </div>
  );
}

function CompanyFormDialog({
  open,
  onOpenChange,
  title,
  description,
  formData,
  setFormData,
  onSubmit,
  isPending,
  submitLabel,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  formData: CompanyFormData;
  setFormData: React.Dispatch<React.SetStateAction<CompanyFormData>>;
  onSubmit: () => void;
  isPending: boolean;
  submitLabel: string;
}) {
  const update = (field: keyof CompanyFormData, value: string) =>
    setFormData((prev) => ({ ...prev, [field]: value }));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-2">
          {/* Basic Info */}
          <div>
            <h4 className="text-sm font-semibold mb-3 text-muted-foreground uppercase tracking-wide">
              Basic Information
            </h4>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium">
                  Company Name <span className="text-destructive">*</span>
                </label>
                <Input
                  placeholder="Acme Inc."
                  value={formData.name}
                  onChange={(e) => update("name", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Legal Name</label>
                <Input
                  placeholder="Acme Inc. LLC"
                  value={formData.legalName}
                  onChange={(e) => update("legalName", e.target.value)}
                />
              </div>
              <div className="sm:col-span-2 space-y-2">
                <label className="text-sm font-medium">Description</label>
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  placeholder="Tell us about your company..."
                  value={formData.description}
                  onChange={(e) => update("description", e.target.value)}
                />
              </div>
            </div>
          </div>

          <Separator />

          {/* Classification */}
          <div>
            <h4 className="text-sm font-semibold mb-3 text-muted-foreground uppercase tracking-wide">
              Classification
            </h4>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="space-y-2">
                <label className="text-sm font-medium">Industry</label>
                <Select value={formData.industry} onValueChange={(v) => update("industry", v)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select industry" />
                  </SelectTrigger>
                  <SelectContent>
                    {INDUSTRIES.map((ind) => (
                      <SelectItem key={ind} value={ind}>
                        {ind}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Company Type</label>
                <Select value={formData.companyType} onValueChange={(v) => update("companyType", v)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    {COMPANY_TYPES.map((t) => (
                      <SelectItem key={t} value={t}>
                        {t}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Company Size</label>
                <Select value={formData.companySize} onValueChange={(v) => update("companySize", v)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select size" />
                  </SelectTrigger>
                  <SelectContent>
                    {COMPANY_SIZES.map((s) => (
                      <SelectItem key={s} value={s}>
                        {s}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          <Separator />

          {/* Contact */}
          <div>
            <h4 className="text-sm font-semibold mb-3 text-muted-foreground uppercase tracking-wide">
              Contact Information
            </h4>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm font-medium">Email</label>
                <Input
                  type="email"
                  placeholder="info@company.com"
                  value={formData.email}
                  onChange={(e) => update("email", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Phone</label>
                <Input
                  placeholder="+1 (555) 123-4567"
                  value={formData.phone}
                  onChange={(e) => update("phone", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Website</label>
                <Input
                  placeholder="https://company.com"
                  value={formData.website}
                  onChange={(e) => update("website", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Founded Year</label>
                <Input
                  type="number"
                  placeholder="2020"
                  value={formData.foundedYear}
                  onChange={(e) => update("foundedYear", e.target.value)}
                />
              </div>
            </div>
          </div>

          <Separator />

          {/* Location */}
          <div>
            <h4 className="text-sm font-semibold mb-3 text-muted-foreground uppercase tracking-wide">
              Location
            </h4>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="sm:col-span-3 space-y-2">
                <label className="text-sm font-medium">Address</label>
                <Input
                  placeholder="123 Business St."
                  value={formData.address}
                  onChange={(e) => update("address", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">City</label>
                <Input
                  placeholder="San Francisco"
                  value={formData.city}
                  onChange={(e) => update("city", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Country</label>
                <Input
                  placeholder="United States"
                  value={formData.country}
                  onChange={(e) => update("country", e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Logo URL</label>
                <Input
                  placeholder="https://..."
                  value={formData.logoUrl}
                  onChange={(e) => update("logoUrl", e.target.value)}
                />
              </div>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={onSubmit} disabled={isPending}>
            {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {submitLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
