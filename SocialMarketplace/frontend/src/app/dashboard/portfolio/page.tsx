"use client";

import { useState, useEffect } from "react";
import { motion } from "framer-motion";
import type { Portfolio } from "@/types/portfolio";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Eye, 
  EyeOff, 
  ExternalLink, 
  Copy, 
  Check,
  User as UserIcon,
  Briefcase,
  GraduationCap,
  Code,
  Award,
  FolderGit2,
  FileText,
  Plus,
  Globe,
  Loader2,
} from "lucide-react";
import Link from "next/link";
import { useAuthStore } from "@/store/auth-store";
import { portfolioApi } from "@/lib/api";
import { toast } from "sonner";
import { PersonalInfoForm } from "@/components/dashboard/personal-info-form";
import { ExperienceSection } from "@/components/dashboard/experience-section";
import { EducationSection } from "@/components/dashboard/education-section";
import { SkillsSection } from "@/components/dashboard/skills-section";
import { ProjectsSection } from "@/components/dashboard/projects-section";
import { CertificationsSection } from "@/components/dashboard/certifications-section";
import { AIResumeImport } from "@/components/dashboard/ai-resume-import";

function parsePortfolioData(raw: Record<string, unknown>): Portfolio {
  return {
    id: raw.id as string,
    userId: raw.userId as string,
    slug: raw.slug as string,
    isPublic: raw.isPublic as boolean,
    theme: (raw.theme as Portfolio["theme"]) || "dark",
    personalInfo: typeof raw.personalInfo === "string" ? JSON.parse(raw.personalInfo) : raw.personalInfo || { fullName: "", title: "", email: "", location: "", bio: "", socialLinks: [] },
    education: typeof raw.education === "string" ? JSON.parse(raw.education) : raw.education || [],
    experience: typeof raw.experience === "string" ? JSON.parse(raw.experience) : raw.experience || [],
    skills: typeof raw.skills === "string" ? JSON.parse(raw.skills) : raw.skills || [],
    roles: typeof raw.roles === "string" ? JSON.parse(raw.roles) : raw.roles || [],
    certifications: typeof raw.certifications === "string" ? JSON.parse(raw.certifications) : raw.certifications || [],
    projects: typeof raw.projects === "string" ? JSON.parse(raw.projects) : raw.projects || [],
    achievements: typeof raw.achievements === "string" ? JSON.parse(raw.achievements) : raw.achievements || [],
    languages: typeof raw.languages === "string" ? JSON.parse(raw.languages) : raw.languages || [],
    resumes: typeof raw.resumes === "string" ? JSON.parse(raw.resumes) : raw.resumes || [],
    createdAt: raw.createdAt as string || new Date().toISOString(),
    updatedAt: raw.updatedAt as string || new Date().toISOString(),
  } as Portfolio;
}

function serializePortfolioForApi(portfolio: Portfolio): Record<string, unknown> {
  return {
    personalInfo: JSON.stringify(portfolio.personalInfo),
    education: JSON.stringify(portfolio.education),
    experience: JSON.stringify(portfolio.experience),
    skills: JSON.stringify(portfolio.skills),
    roles: JSON.stringify(portfolio.roles),
    certifications: JSON.stringify(portfolio.certifications),
    projects: JSON.stringify(portfolio.projects),
    achievements: JSON.stringify(portfolio.achievements),
    languages: JSON.stringify(portfolio.languages),
    resumes: JSON.stringify(portfolio.resumes),
    isPublic: portfolio.isPublic,
  };
}

export default function PortfolioPage() {
  const { user } = useAuthStore();
  const [portfolio, setPortfolio] = useState<Portfolio | null>(null);
  const [loading, setLoading] = useState(true);
  const [copied, setCopied] = useState(false);
  const [isToggling, setIsToggling] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [portfolioUrl, setPortfolioUrl] = useState("");

  useEffect(() => {
    loadPortfolio();
  }, []);

  useEffect(() => {
    if (portfolio?.slug && typeof window !== "undefined") {
      setPortfolioUrl(`${window.location.origin}/p/${portfolio.slug}`);
    }
  }, [portfolio?.slug]);

  const loadPortfolio = async () => {
    try {
      const { data } = await portfolioApi.getMe();
      if (data) {
        setPortfolio(parsePortfolioData(data));
      }
    } catch {
      setPortfolio(null);
    } finally {
      setLoading(false);
    }
  };

  const createPortfolio = async () => {
    try {
      const slug = user?.username || `user-${Date.now()}`;
      await portfolioApi.create({
        slug,
        isPublic: false,
        theme: "dark",
        personalInfo: JSON.stringify({
          fullName: `${user?.firstName || ""} ${user?.lastName || ""}`.trim(),
          title: "",
          email: user?.email || "",
          phone: "",
          location: "",
          bio: "",
          profileImage: "",
          socialLinks: [],
        }),
      });
      await loadPortfolio();
    } catch (err) {
      toast.error("Failed to create portfolio");
    }
  };

  const savePortfolio = async (updatedPortfolio: Portfolio) => {
    setIsSaving(true);
    try {
      await portfolioApi.update(updatedPortfolio.id, serializePortfolioForApi(updatedPortfolio));
      setPortfolio(updatedPortfolio);
    } catch {
      toast.error("Failed to save changes");
    } finally {
      setIsSaving(false);
    }
  };

  const handleToggleVisibility = async () => {
    if (!portfolio) return;
    setIsToggling(true);
    try {
      const newIsPublic = !portfolio.isPublic;
      await portfolioApi.update(portfolio.id, { isPublic: newIsPublic });
      setPortfolio({ ...portfolio, isPublic: newIsPublic });
      toast.success(newIsPublic ? "Portfolio is now public" : "Portfolio is now private");
    } catch {
      toast.error("Failed to update visibility");
    } finally {
      setIsToggling(false);
    }
  };

  const handleCopyUrl = async () => {
    const fullUrl = `${window.location.origin}/p/${portfolio?.slug}`;
    await navigator.clipboard.writeText(fullUrl);
    setCopied(true);
    toast.success("URL copied to clipboard!");
    setTimeout(() => setCopied(false), 2000);
  };

  const handlePortfolioUpdate = (updated: Portfolio) => {
    setPortfolio(updated);
    savePortfolio(updated);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  if (!portfolio) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <Globe className="h-16 w-16 text-muted-foreground/50" />
        <h2 className="mt-4 text-2xl font-bold">Create Your Portfolio</h2>
        <p className="mt-2 text-center text-muted-foreground max-w-md">
          Build a professional portfolio to showcase your skills, experience,
          projects, and education. Share it with potential clients and employers.
        </p>
        <Button className="mt-6" size="lg" onClick={createPortfolio}>
          <Plus className="mr-2 h-5 w-5" />
          Create Portfolio
        </Button>
      </div>
    );
  }

  const tabs = [
    { id: "personal", label: "Personal Info", icon: UserIcon },
    { id: "experience", label: "Experience", icon: Briefcase },
    { id: "education", label: "Education", icon: GraduationCap },
    { id: "skills", label: "Skills & Roles", icon: Code },
    { id: "projects", label: "Projects", icon: FolderGit2 },
    { id: "certifications", label: "Certifications", icon: Award },
    { id: "resumes", label: "Resumes", icon: FileText },
  ];

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-8">
          <div>
            <h1 className="text-3xl font-bold">My Portfolio</h1>
            <p className="text-muted-foreground mt-1">Manage your portfolio and resume</p>
          </div>
          <div className="flex items-center gap-3">
            {isSaving && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Loader2 className="h-4 w-4 animate-spin" />
                Saving...
              </div>
            )}
            <Button
              variant={portfolio.isPublic ? "default" : "secondary"}
              onClick={handleToggleVisibility}
              disabled={isToggling}
            >
              {portfolio.isPublic ? (
                <>
                  <Eye className="mr-2 h-4 w-4" />
                  Public
                </>
              ) : (
                <>
                  <EyeOff className="mr-2 h-4 w-4" />
                  Private
                </>
              )}
            </Button>
          </div>
        </div>

        {/* Portfolio URL Card */}
        <Card className="mb-8 border-primary/20 bg-primary/5">
          <CardContent className="py-4">
            <div className="flex flex-col md:flex-row md:items-center gap-4">
              <div className="flex-1">
                <p className="text-sm text-muted-foreground mb-1">Your Portfolio URL</p>
                <div className="flex items-center gap-2">
                  <code className="text-sm md:text-base font-mono bg-background px-3 py-1.5 rounded-md border flex-1 truncate">
                    {portfolioUrl || `/p/${portfolio.slug}`}
                  </code>
                  <Button size="icon" variant="ghost" onClick={handleCopyUrl}>
                    {copied ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
                  </Button>
                  {portfolio.isPublic && (
                    <Button size="icon" variant="ghost" asChild>
                      <a href={`/p/${portfolio.slug}`} target="_blank" rel="noopener noreferrer">
                        <ExternalLink className="h-4 w-4" />
                      </a>
                    </Button>
                  )}
                </div>
              </div>
              <Badge variant={portfolio.isPublic ? "default" : "secondary"}>
                {portfolio.isPublic ? "Live" : "Draft"}
              </Badge>
            </div>
          </CardContent>
        </Card>

        {/* Main Content Tabs */}
        <Tabs defaultValue="personal" className="space-y-6">
          <TabsList className="flex flex-wrap h-auto gap-2 bg-transparent p-0">
            {tabs.map((tab) => (
              <TabsTrigger
                key={tab.id}
                value={tab.id}
                className="data-[state=active]:bg-primary data-[state=active]:text-primary-foreground px-4 py-2 rounded-lg border"
              >
                <tab.icon className="h-4 w-4 mr-2" />
                <span className="hidden sm:inline">{tab.label}</span>
              </TabsTrigger>
            ))}
          </TabsList>

          <TabsContent value="personal" className="mt-6 space-y-6">
            <AIResumeImport
              portfolio={portfolio}
              onImport={(updates) => handlePortfolioUpdate({ ...portfolio, ...updates })}
            />
            <PersonalInfoForm portfolio={portfolio} onUpdate={handlePortfolioUpdate} />
          </TabsContent>

          <TabsContent value="experience" className="mt-6">
            <ExperienceSection
              experiences={portfolio.experience}
              onUpdate={(experience) => handlePortfolioUpdate({ ...portfolio, experience })}
            />
          </TabsContent>

          <TabsContent value="education" className="mt-6">
            <EducationSection
              education={portfolio.education}
              onUpdate={(education) => handlePortfolioUpdate({ ...portfolio, education })}
            />
          </TabsContent>

          <TabsContent value="skills" className="mt-6">
            <SkillsSection
              skills={portfolio.skills}
              roles={portfolio.roles}
              onUpdateSkills={(skills) => handlePortfolioUpdate({ ...portfolio, skills })}
              onUpdateRoles={(roles) => handlePortfolioUpdate({ ...portfolio, roles })}
            />
          </TabsContent>

          <TabsContent value="projects" className="mt-6">
            <ProjectsSection
              projects={portfolio.projects}
              onUpdate={(projects) => handlePortfolioUpdate({ ...portfolio, projects })}
            />
          </TabsContent>

          <TabsContent value="certifications" className="mt-6">
            <CertificationsSection
              certifications={portfolio.certifications}
              onUpdate={(certifications) => handlePortfolioUpdate({ ...portfolio, certifications })}
            />
          </TabsContent>

          <TabsContent value="resumes" className="mt-6">
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <FileText className="h-12 w-12 text-muted-foreground/50" />
                <h3 className="mt-4 text-lg font-semibold">Resume Management</h3>
                <p className="text-muted-foreground text-center mt-2 max-w-md">
                  Create and manage multiple resumes with different templates. Your resumes use your portfolio data automatically.
                </p>
                <Button className="mt-4" asChild>
                  <Link href="/dashboard/resume">
                    <FileText className="mr-2 h-4 w-4" />
                    Go to Resume Manager
                  </Link>
                </Button>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </motion.div>
    </div>
  );
}
