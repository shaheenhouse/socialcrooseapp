"use client";

import { useState, useEffect, useCallback } from "react";
import { toast } from "sonner";
import type { Portfolio, Resume, ResumeData } from "@/types/portfolio";
import { resumeApi, portfolioApi } from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";
import { ResumeModal } from "@/components/resume/resume-modal";
import { ResumeEditor } from "@/components/resume/resume-editor";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  FileText,
  Plus,
  MoreVertical,
  Eye,
  Pencil,
  Copy,
  Trash2,
  Star,
  Lock,
  RefreshCw,
  Loader2,
} from "lucide-react";

function portfolioToResumeData(portfolio: Portfolio): ResumeData {
  return {
    personalInfo: JSON.parse(JSON.stringify(portfolio.personalInfo)),
    education: JSON.parse(JSON.stringify(portfolio.education)),
    experience: JSON.parse(JSON.stringify(portfolio.experience)),
    skills: JSON.parse(JSON.stringify(portfolio.skills)),
    certifications: JSON.parse(JSON.stringify(portfolio.certifications)),
    projects: JSON.parse(JSON.stringify(portfolio.projects)),
    languages: JSON.parse(JSON.stringify(portfolio.languages || [])),
  };
}

function resumeToPortfolio(portfolio: Portfolio, resume: Resume): Portfolio {
  if (resume.isStandard || !resume.data) {
    return portfolio;
  }
  return {
    ...portfolio,
    personalInfo: resume.data.personalInfo,
    education: resume.data.education,
    experience: resume.data.experience,
    skills: resume.data.skills,
    certifications: resume.data.certifications,
    projects: resume.data.projects,
    languages: resume.data.languages,
  };
}

const emptyPortfolio: Portfolio = {
  id: "",
  userId: "",
  slug: "",
  isPublic: false,
  theme: "dark",
  personalInfo: {
    fullName: "",
    title: "",
    email: "",
    location: "",
    bio: "",
    socialLinks: [],
  },
  education: [],
  experience: [],
  skills: [],
  roles: [],
  certifications: [],
  projects: [],
  achievements: [],
  languages: [],
  resumes: [],
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

export default function ResumePage() {
  const { user } = useAuthStore();
  const [resumes, setResumes] = useState<Resume[]>([]);
  const [portfolio, setPortfolio] = useState<Portfolio>(emptyPortfolio);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [showPreview, setShowPreview] = useState(false);
  const [previewResumeId, setPreviewResumeId] = useState<string | null>(null);
  const [editingResumeId, setEditingResumeId] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      const [resumeRes, portfolioRes] = await Promise.all([
        resumeApi.getMe(),
        portfolioApi.getMe(),
      ]);

      const rawResumes = resumeRes.data?.data || resumeRes.data || [];
      const mapped: Resume[] = (Array.isArray(rawResumes) ? rawResumes : []).map((r: Record<string, unknown>) => ({
        id: (r.id || r._id || "") as string,
        name: (r.name || r.title || "Untitled") as string,
        templateId: ((r.templateId || r.template || "classic") as Resume["templateId"]),
        isActive: (r.isActive ?? false) as boolean,
        isStandard: (r.isStandard ?? true) as boolean,
        data: r.data as ResumeData | undefined,
        resumeImage: r.resumeImage as string | undefined,
        createdAt: (r.createdAt || new Date().toISOString()) as string,
        updatedAt: (r.updatedAt || new Date().toISOString()) as string,
      }));

      setResumes(mapped);

      const pData = portfolioRes.data?.data || portfolioRes.data;
      if (pData && typeof pData === "object") {
        const parseField = (val: unknown, fallback: unknown = []) => {
          if (typeof val === "string") { try { return JSON.parse(val); } catch { return fallback; } }
          return val || fallback;
        };
        const pi = parseField(pData.personalInfo, {});
        setPortfolio({
          ...emptyPortfolio,
          ...pData,
          personalInfo: { ...emptyPortfolio.personalInfo, ...(typeof pi === "object" && pi ? pi : {}) },
          education: parseField(pData.education) as Portfolio["education"],
          experience: parseField(pData.experience) as Portfolio["experience"],
          skills: parseField(pData.skills) as Portfolio["skills"],
          roles: parseField(pData.roles) as Portfolio["roles"],
          certifications: parseField(pData.certifications) as Portfolio["certifications"],
          projects: parseField(pData.projects) as Portfolio["projects"],
          achievements: parseField(pData.achievements) as Portfolio["achievements"],
          languages: parseField(pData.languages) as Portfolio["languages"],
          resumes: mapped,
        });
      }
    } catch {
      // Portfolio or resume service may not exist yet
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleCreateStandard = async () => {
    setActionLoading("create-standard");
    try {
      const res = await resumeApi.create({
        name: `Resume ${resumes.length + 1} (Profile)`,
        templateId: "classic",
        isActive: resumes.length === 0,
        isStandard: true,
      });
      toast.success("Standard resume created");
      await loadData();
      return res;
    } catch {
      toast.error("Failed to create resume");
    } finally {
      setActionLoading(null);
    }
  };

  const handleCreateCustom = async () => {
    setActionLoading("create-custom");
    try {
      const data = portfolioToResumeData(portfolio);
      await resumeApi.create({
        name: `Resume ${resumes.length + 1}`,
        templateId: "classic",
        isActive: false,
        isStandard: false,
        data,
      });
      toast.success("Custom resume created", {
        description: "Data copied from your profile. Click Edit to customize.",
      });
      await loadData();
    } catch {
      toast.error("Failed to create resume");
    } finally {
      setActionLoading(null);
    }
  };

  const handleSetActive = async (id: string) => {
    setActionLoading(id);
    try {
      await Promise.all(
        resumes.map((r) =>
          resumeApi.update(r.id, { isActive: r.id === id })
        )
      );
      setResumes((prev) =>
        prev.map((r) => ({ ...r, isActive: r.id === id }))
      );
      toast.success("Active resume updated");
    } catch {
      toast.error("Failed to set active resume");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = async (id: string) => {
    const target = resumes.find((r) => r.id === id);
    if (!target) return;
    if (resumes.length <= 1) {
      toast.error("Cannot delete the only resume");
      return;
    }

    setActionLoading(id);
    try {
      await resumeApi.delete(id);
      toast.success("Resume deleted");
      await loadData();
    } catch {
      toast.error("Failed to delete resume");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDuplicate = async (id: string) => {
    const source = resumes.find((r) => r.id === id);
    if (!source) return;

    setActionLoading(id);
    try {
      const newData: Record<string, unknown> = {
        name: `${source.name} (Copy)`,
        templateId: source.templateId,
        isActive: false,
        isStandard: false,
        data: source.isStandard
          ? portfolioToResumeData(portfolio)
          : source.data || portfolioToResumeData(portfolio),
      };
      if (source.resumeImage) newData.resumeImage = source.resumeImage;

      await resumeApi.create(newData);
      toast.success("Resume duplicated");
      await loadData();
    } catch {
      toast.error("Failed to duplicate resume");
    } finally {
      setActionLoading(null);
    }
  };

  const handleTemplateChange = async (id: string, templateId: string) => {
    try {
      await resumeApi.update(id, { templateId });
      setResumes((prev) =>
        prev.map((r) =>
          r.id === id
            ? { ...r, templateId: templateId as Resume["templateId"], updatedAt: new Date().toISOString() }
            : r
        )
      );
    } catch {
      toast.error("Failed to update template");
    }
  };

  const handleNameChange = async (id: string, name: string) => {
    setResumes((prev) =>
      prev.map((r) => (r.id === id ? { ...r, name } : r))
    );
    try {
      await resumeApi.update(id, { name });
    } catch {
      // Revert on error — reload
      await loadData();
    }
  };

  const handleResumeDataSave = async (id: string, data: ResumeData) => {
    setActionLoading(id);
    try {
      await resumeApi.update(id, { data });
      setResumes((prev) =>
        prev.map((r) =>
          r.id === id ? { ...r, data, updatedAt: new Date().toISOString() } : r
        )
      );
      toast.success("Resume data saved");
    } catch {
      toast.error("Failed to save resume data");
    } finally {
      setActionLoading(null);
    }
  };

  const handleSyncFromProfile = async (id: string) => {
    setActionLoading(id);
    try {
      const freshData = portfolioToResumeData(portfolio);
      await resumeApi.update(id, { data: freshData });
      setResumes((prev) =>
        prev.map((r) =>
          r.id === id
            ? { ...r, data: freshData, updatedAt: new Date().toISOString() }
            : r
        )
      );
      toast.success("Resume synced from profile");
    } catch {
      toast.error("Failed to sync from profile");
    } finally {
      setActionLoading(null);
    }
  };

  // Preview logic
  const previewResume = previewResumeId
    ? resumes.find((r) => r.id === previewResumeId)
    : resumes.find((r) => r.isActive) || resumes[0];
  const previewPortfolio = previewResume
    ? resumeToPortfolio(portfolio, previewResume)
    : portfolio;

  // Editing resume
  const editingResume = editingResumeId
    ? resumes.find((r) => r.id === editingResumeId)
    : null;
  const editingResumeData = editingResume
    ? editingResume.isStandard
      ? portfolioToResumeData(portfolio)
      : editingResume.data || portfolioToResumeData(portfolio)
    : null;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Resumes</h1>
          <p className="text-muted-foreground">
            Create and manage multiple resume versions with different templates
          </p>
        </div>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button disabled={actionLoading === "create-standard" || actionLoading === "create-custom"}>
              {actionLoading?.startsWith("create") ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Plus className="mr-2 h-4 w-4" />
              )}
              New Resume
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={handleCreateStandard}>
              <Lock className="mr-2 h-4 w-4" />
              Standard (Profile Data)
            </DropdownMenuItem>
            <DropdownMenuItem onClick={handleCreateCustom}>
              <Pencil className="mr-2 h-4 w-4" />
              Custom (Editable Copy)
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {resumes.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-20">
            <FileText className="h-16 w-16 text-muted-foreground/50" />
            <h2 className="mt-4 text-xl font-semibold">No Resumes Yet</h2>
            <p className="mt-2 text-center text-muted-foreground max-w-md">
              Create your first resume. <strong>Standard</strong> resumes always
              use your profile data. <strong>Custom</strong> resumes let you
              tailor content for specific applications.
            </p>
            <div className="flex gap-3 mt-6">
              <Button
                onClick={handleCreateStandard}
                disabled={!!actionLoading}
              >
                <Lock className="mr-2 h-4 w-4" />
                Standard Resume
              </Button>
              <Button
                variant="outline"
                onClick={handleCreateCustom}
                disabled={!!actionLoading}
              >
                <Pencil className="mr-2 h-4 w-4" />
                Custom Resume
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>All Resumes</CardTitle>
            <CardDescription>
              <strong>Standard</strong> resumes always reflect your profile
              data. <strong>Custom</strong> resumes have their own editable
              data (initially copied from your profile).
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {resumes.map((resume) => (
              <div
                key={resume.id}
                className={`flex flex-col sm:flex-row sm:items-center gap-3 p-4 rounded-lg border transition-all ${
                  resume.isActive
                    ? "border-primary bg-primary/5 ring-1 ring-primary/20"
                    : "border-border hover:border-primary/40"
                }`}
              >
                {/* Left: info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <input
                      type="text"
                      value={resume.name}
                      onChange={(e) =>
                        handleNameChange(resume.id, e.target.value)
                      }
                      className="font-semibold text-sm bg-transparent border-none outline-none focus:underline max-w-[220px]"
                    />
                    {resume.isActive && (
                      <Badge variant="default" className="text-[10px]">
                        <Star className="h-2.5 w-2.5 mr-0.5" /> Active
                      </Badge>
                    )}
                    {resume.isStandard ? (
                      <Badge
                        variant="secondary"
                        className="text-[10px] gap-0.5"
                      >
                        <Lock className="h-2.5 w-2.5" /> Profile Data
                      </Badge>
                    ) : (
                      <Badge
                        variant="outline"
                        className="text-[10px] gap-0.5"
                      >
                        <Pencil className="h-2.5 w-2.5" /> Custom Data
                      </Badge>
                    )}
                  </div>
                  <div className="flex items-center gap-3 mt-1">
                    <select
                      value={resume.templateId}
                      onChange={(e) =>
                        handleTemplateChange(resume.id, e.target.value)
                      }
                      className="text-xs bg-muted rounded px-2 py-1 border-none outline-none cursor-pointer"
                    >
                      <option value="classic">Classic Template</option>
                      <option value="modern">Modern Template</option>
                      <option value="minimal">Minimal Template</option>
                    </select>
                    <span className="text-[10px] text-muted-foreground">
                      Updated{" "}
                      {new Date(resume.updatedAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>

                {/* Right: actions */}
                <div className="flex items-center gap-2 shrink-0">
                  {!resume.isStandard && (
                    <Button
                      size="sm"
                      variant="outline"
                      className="text-xs gap-1"
                      onClick={() => setEditingResumeId(resume.id)}
                    >
                      <Pencil className="h-3 w-3" /> Edit Data
                    </Button>
                  )}
                  <Button
                    size="sm"
                    variant="outline"
                    className="text-xs"
                    onClick={() => {
                      setPreviewResumeId(resume.id);
                      setShowPreview(true);
                    }}
                  >
                    <Eye className="h-3 w-3 mr-1" /> Preview
                  </Button>

                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button
                        size="icon"
                        variant="ghost"
                        className="h-8 w-8"
                        disabled={actionLoading === resume.id}
                      >
                        {actionLoading === resume.id ? (
                          <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                          <MoreVertical className="h-4 w-4" />
                        )}
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      {!resume.isActive && (
                        <DropdownMenuItem
                          onClick={() => handleSetActive(resume.id)}
                        >
                          <Star className="mr-2 h-4 w-4" />
                          Set as Active
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuItem
                        onClick={() => handleDuplicate(resume.id)}
                      >
                        <Copy className="mr-2 h-4 w-4" />
                        Duplicate
                      </DropdownMenuItem>
                      {!resume.isStandard && (
                        <DropdownMenuItem
                          onClick={() => handleSyncFromProfile(resume.id)}
                        >
                          <RefreshCw className="mr-2 h-4 w-4" />
                          Sync from Profile
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      {resumes.length > 1 && (
                        <DropdownMenuItem
                          className="text-destructive focus:text-destructive"
                          onClick={() => handleDelete(resume.id)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Delete
                        </DropdownMenuItem>
                      )}
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </div>
            ))}

            <p className="text-xs text-muted-foreground pt-2 border-t">
              Tip: Standard resumes always reflect your profile. Create a
              custom resume to tailor content for specific job applications.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Resume Preview Modal */}
      {previewResume && (
        <ResumeModal
          open={showPreview}
          onOpenChange={setShowPreview}
          portfolio={previewPortfolio}
          defaultTemplate={previewResume.templateId || "classic"}
          resumeImage={previewResume.resumeImage}
        />
      )}

      {/* Resume Data Editor */}
      {editingResume && editingResumeData && (
        <ResumeEditor
          open={!!editingResumeId}
          onOpenChange={(open) => {
            if (!open) setEditingResumeId(null);
          }}
          data={editingResumeData}
          resumeName={editingResume.name}
          onSave={(data) => handleResumeDataSave(editingResume.id, data)}
        />
      )}
    </div>
  );
}
