"use client";

import { useState, useEffect } from "react";
import { motion } from "framer-motion";
import {
  Globe,
  Edit,
  Eye,
  EyeOff,
  Plus,
  Briefcase,
  GraduationCap,
  Code,
  Award,
  Languages,
  FolderKanban,
  User,
  Save,
  ExternalLink,
  Trash2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { Switch } from "@/components/ui/switch";
import { useAuthStore } from "@/store/auth-store";
import { portfolioApi } from "@/lib/api";
import Link from "next/link";

interface PortfolioData {
  id: string;
  userId: string;
  slug: string;
  isPublic: boolean;
  theme: string;
  personalInfo: string;
  education: string;
  experience: string;
  skills: string;
  roles: string;
  certifications: string;
  projects: string;
  achievements: string;
  languages: string;
  resumes: string;
}

export default function PortfolioPage() {
  const { user } = useAuthStore();
  const [portfolio, setPortfolio] = useState<PortfolioData | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);

  useEffect(() => {
    loadPortfolio();
  }, []);

  const loadPortfolio = async () => {
    try {
      const { data } = await portfolioApi.getMe();
      setPortfolio(data);
    } catch {
      setPortfolio(null);
    } finally {
      setLoading(false);
    }
  };

  const createPortfolio = async () => {
    try {
      const slug = user?.username || `user-${Date.now()}`;
      const { data } = await portfolioApi.create({
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
      console.error("Failed to create portfolio:", err);
    }
  };

  const toggleVisibility = async () => {
    if (!portfolio) return;
    try {
      await portfolioApi.update(portfolio.id, {
        isPublic: !portfolio.isPublic,
      });
      setPortfolio({ ...portfolio, isPublic: !portfolio.isPublic });
    } catch (err) {
      console.error("Failed to update visibility:", err);
    }
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

  const personalInfo = JSON.parse(portfolio.personalInfo || "{}");
  const education = JSON.parse(portfolio.education || "[]");
  const experience = JSON.parse(portfolio.experience || "[]");
  const skills = JSON.parse(portfolio.skills || "[]");
  const roles = JSON.parse(portfolio.roles || "[]");
  const certifications = JSON.parse(portfolio.certifications || "[]");
  const projects = JSON.parse(portfolio.projects || "[]");
  const achievements = JSON.parse(portfolio.achievements || "[]");
  const languages = JSON.parse(portfolio.languages || "[]");

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Portfolio</h1>
          <p className="text-muted-foreground">
            Manage your professional portfolio
          </p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            {portfolio.isPublic ? (
              <Eye className="h-4 w-4 text-green-500" />
            ) : (
              <EyeOff className="h-4 w-4 text-muted-foreground" />
            )}
            <span className="text-sm">
              {portfolio.isPublic ? "Public" : "Private"}
            </span>
            <Switch
              checked={portfolio.isPublic}
              onCheckedChange={toggleVisibility}
            />
          </div>
          {portfolio.isPublic && (
            <Button variant="outline" size="sm" asChild>
              <a
                href={`/p/${portfolio.slug}`}
                target="_blank"
                rel="noopener noreferrer"
              >
                <ExternalLink className="mr-2 h-4 w-4" />
                View Public
              </a>
            </Button>
          )}
          <Link href="/dashboard/resume">
            <Button variant="outline" size="sm">
              Resumes
            </Button>
          </Link>
        </div>
      </div>

      <Tabs defaultValue="overview">
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="experience">Experience</TabsTrigger>
          <TabsTrigger value="skills">Skills</TabsTrigger>
          <TabsTrigger value="projects">Projects</TabsTrigger>
          <TabsTrigger value="education">Education</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6 space-y-6">
          {/* Personal Info Card */}
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <div>
                <CardTitle className="flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Personal Information
                </CardTitle>
                <CardDescription>
                  Your basic profile information
                </CardDescription>
              </div>
              <Button variant="outline" size="sm">
                <Edit className="mr-2 h-4 w-4" />
                Edit
              </Button>
            </CardHeader>
            <CardContent className="grid gap-4 md:grid-cols-2">
              <div>
                <p className="text-sm text-muted-foreground">Full Name</p>
                <p className="font-medium">
                  {personalInfo.fullName || "Not set"}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Title</p>
                <p className="font-medium">
                  {personalInfo.title || "Not set"}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Email</p>
                <p className="font-medium">
                  {personalInfo.email || "Not set"}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Location</p>
                <p className="font-medium">
                  {personalInfo.location || "Not set"}
                </p>
              </div>
              <div className="md:col-span-2">
                <p className="text-sm text-muted-foreground">Bio</p>
                <p className="font-medium">
                  {personalInfo.bio || "No bio added yet"}
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Quick Stats */}
          <div className="grid gap-4 md:grid-cols-4">
            <Card>
              <CardContent className="flex items-center gap-3 p-4">
                <Briefcase className="h-8 w-8 text-blue-500" />
                <div>
                  <p className="text-2xl font-bold">{experience.length}</p>
                  <p className="text-sm text-muted-foreground">
                    Experiences
                  </p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="flex items-center gap-3 p-4">
                <Code className="h-8 w-8 text-green-500" />
                <div>
                  <p className="text-2xl font-bold">{skills.length}</p>
                  <p className="text-sm text-muted-foreground">Skills</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="flex items-center gap-3 p-4">
                <FolderKanban className="h-8 w-8 text-purple-500" />
                <div>
                  <p className="text-2xl font-bold">{projects.length}</p>
                  <p className="text-sm text-muted-foreground">Projects</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="flex items-center gap-3 p-4">
                <Award className="h-8 w-8 text-amber-500" />
                <div>
                  <p className="text-2xl font-bold">
                    {certifications.length}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Certifications
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Roles */}
          {roles.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Roles</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2">
                  {roles.map((role: { id: string; title: string; level: string }) => (
                    <Badge key={role.id} variant="secondary" className="px-3 py-1">
                      {role.title} - {role.level}
                    </Badge>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="experience" className="mt-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Work Experience</h2>
            <Button size="sm">
              <Plus className="mr-2 h-4 w-4" />
              Add Experience
            </Button>
          </div>
          {experience.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Briefcase className="h-12 w-12 text-muted-foreground/50" />
                <p className="mt-4 text-muted-foreground">
                  No experience added yet
                </p>
              </CardContent>
            </Card>
          ) : (
            experience.map((exp: { id: string; title: string; company: string; location: string; startDate: string; endDate?: string; current: boolean; description: string; technologies?: string[] }) => (
              <Card key={exp.id}>
                <CardContent className="p-6">
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="text-lg font-semibold">{exp.title}</h3>
                      <p className="text-muted-foreground">
                        {exp.company} - {exp.location}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {exp.startDate} - {exp.current ? "Present" : exp.endDate}
                      </p>
                    </div>
                    <Button variant="ghost" size="icon">
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                  <p className="mt-3 text-sm">{exp.description}</p>
                  {exp.technologies && exp.technologies.length > 0 && (
                    <div className="mt-3 flex flex-wrap gap-1">
                      {exp.technologies.map((tech: string) => (
                        <Badge key={tech} variant="outline" className="text-xs">
                          {tech}
                        </Badge>
                      ))}
                    </div>
                  )}
                </CardContent>
              </Card>
            ))
          )}
        </TabsContent>

        <TabsContent value="skills" className="mt-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Skills</h2>
            <Button size="sm">
              <Plus className="mr-2 h-4 w-4" />
              Add Skill
            </Button>
          </div>
          {skills.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Code className="h-12 w-12 text-muted-foreground/50" />
                <p className="mt-4 text-muted-foreground">
                  No skills added yet
                </p>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
              {skills.map((skill: { id: string; name: string; level: string; category: string }) => (
                <Card key={skill.id}>
                  <CardContent className="flex items-center justify-between p-4">
                    <div>
                      <p className="font-medium">{skill.name}</p>
                      <p className="text-sm text-muted-foreground">
                        {skill.category}
                      </p>
                    </div>
                    <Badge
                      variant={
                        skill.level === "expert"
                          ? "default"
                          : skill.level === "proficient"
                          ? "secondary"
                          : "outline"
                      }
                    >
                      {skill.level}
                    </Badge>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}

          <Separator className="my-6" />

          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Languages</h2>
            <Button size="sm" variant="outline">
              <Plus className="mr-2 h-4 w-4" />
              Add Language
            </Button>
          </div>
          <div className="flex flex-wrap gap-2">
            {languages.map((lang: { id: string; name: string; proficiency: string }) => (
              <Badge key={lang.id} variant="secondary" className="px-3 py-1">
                {lang.name} ({lang.proficiency})
              </Badge>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="projects" className="mt-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Projects</h2>
            <Button size="sm">
              <Plus className="mr-2 h-4 w-4" />
              Add Project
            </Button>
          </div>
          {projects.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <FolderKanban className="h-12 w-12 text-muted-foreground/50" />
                <p className="mt-4 text-muted-foreground">
                  No projects added yet
                </p>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              {projects.map((project: { id: string; name: string; description: string; url?: string; technologies?: string[]; highlights?: string[] }) => (
                <Card key={project.id}>
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <CardTitle className="text-lg">{project.name}</CardTitle>
                      {project.url && (
                        <a href={project.url} target="_blank" rel="noopener noreferrer">
                          <ExternalLink className="h-4 w-4 text-muted-foreground hover:text-foreground" />
                        </a>
                      )}
                    </div>
                    <CardDescription>{project.description}</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {project.technologies && project.technologies.length > 0 && (
                      <div className="flex flex-wrap gap-1">
                        {project.technologies.map((tech: string) => (
                          <Badge key={tech} variant="outline" className="text-xs">
                            {tech}
                          </Badge>
                        ))}
                      </div>
                    )}
                    {project.highlights && project.highlights.length > 0 && (
                      <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
                        {project.highlights.map((h: string, i: number) => (
                          <li key={i} className="flex items-center gap-2">
                            <div className="h-1.5 w-1.5 rounded-full bg-primary" />
                            {h}
                          </li>
                        ))}
                      </ul>
                    )}
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="education" className="mt-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Education</h2>
            <Button size="sm">
              <Plus className="mr-2 h-4 w-4" />
              Add Education
            </Button>
          </div>
          {education.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <GraduationCap className="h-12 w-12 text-muted-foreground/50" />
                <p className="mt-4 text-muted-foreground">
                  No education added yet
                </p>
              </CardContent>
            </Card>
          ) : (
            education.map((edu: { id: string; degree: string; institution: string; field: string; startDate: string; endDate?: string; gpa?: string; description?: string }) => (
              <Card key={edu.id}>
                <CardContent className="p-6">
                  <div className="flex items-start justify-between">
                    <div className="flex gap-4">
                      <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-primary/10">
                        <GraduationCap className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <h3 className="font-semibold">{edu.degree}</h3>
                        <p className="text-muted-foreground">
                          {edu.institution} - {edu.field}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {edu.startDate} - {edu.endDate || "Present"}
                          {edu.gpa && ` | GPA: ${edu.gpa}`}
                        </p>
                        {edu.description && (
                          <p className="mt-2 text-sm">{edu.description}</p>
                        )}
                      </div>
                    </div>
                    <Button variant="ghost" size="icon">
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))
          )}

          <Separator className="my-6" />

          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold">Certifications</h2>
            <Button size="sm" variant="outline">
              <Plus className="mr-2 h-4 w-4" />
              Add Certification
            </Button>
          </div>
          {certifications.map((cert: { id: string; name: string; issuer: string; issueDate: string }) => (
            <Card key={cert.id}>
              <CardContent className="flex items-center gap-4 p-4">
                <Award className="h-8 w-8 text-amber-500" />
                <div>
                  <p className="font-medium">{cert.name}</p>
                  <p className="text-sm text-muted-foreground">
                    {cert.issuer} - {cert.issueDate}
                  </p>
                </div>
              </CardContent>
            </Card>
          ))}
        </TabsContent>
      </Tabs>
    </div>
  );
}
