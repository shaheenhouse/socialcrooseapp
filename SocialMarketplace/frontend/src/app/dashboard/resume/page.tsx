"use client";

import { useState, useEffect } from "react";
import {
  FileText,
  Plus,
  Download,
  Eye,
  Edit,
  Trash2,
  Copy,
  MoreVertical,
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { resumeApi } from "@/lib/api";

interface ResumeItem {
  id: string;
  title: string;
  template: string;
  isPublic: boolean;
  pdfUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

const templateColors: Record<string, string> = {
  classic: "bg-blue-500/10 text-blue-600",
  modern: "bg-purple-500/10 text-purple-600",
  minimal: "bg-green-500/10 text-green-600",
  professional: "bg-amber-500/10 text-amber-600",
  creative: "bg-pink-500/10 text-pink-600",
};

export default function ResumePage() {
  const [resumes, setResumes] = useState<ResumeItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadResumes();
  }, []);

  const loadResumes = async () => {
    try {
      const { data } = await resumeApi.getMe();
      setResumes(data.data || []);
    } catch {
      setResumes([]);
    } finally {
      setLoading(false);
    }
  };

  const createResume = async () => {
    try {
      await resumeApi.create({
        title: `Resume ${resumes.length + 1}`,
        template: "modern",
      });
      await loadResumes();
    } catch (err) {
      console.error("Failed to create resume:", err);
    }
  };

  const deleteResume = async (id: string) => {
    try {
      await resumeApi.delete(id);
      setResumes(resumes.filter((r) => r.id !== id));
    } catch (err) {
      console.error("Failed to delete resume:", err);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Resumes</h1>
          <p className="text-muted-foreground">
            Create and manage multiple resume versions
          </p>
        </div>
        <Button onClick={createResume}>
          <Plus className="mr-2 h-4 w-4" />
          New Resume
        </Button>
      </div>

      {resumes.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-20">
            <FileText className="h-16 w-16 text-muted-foreground/50" />
            <h2 className="mt-4 text-xl font-semibold">No Resumes Yet</h2>
            <p className="mt-2 text-center text-muted-foreground max-w-md">
              Create your first resume using one of our professional templates.
              You can create multiple resumes for different job applications.
            </p>
            <Button className="mt-6" onClick={createResume}>
              <Plus className="mr-2 h-4 w-4" />
              Create Your First Resume
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {resumes.map((resume) => (
            <Card key={resume.id} className="group relative overflow-hidden transition-all hover:shadow-lg">
              <div className="absolute right-2 top-2 opacity-0 transition-opacity group-hover:opacity-100">
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon" className="h-8 w-8">
                      <MoreVertical className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem>
                      <Edit className="mr-2 h-4 w-4" />
                      Edit
                    </DropdownMenuItem>
                    <DropdownMenuItem>
                      <Eye className="mr-2 h-4 w-4" />
                      Preview
                    </DropdownMenuItem>
                    <DropdownMenuItem>
                      <Copy className="mr-2 h-4 w-4" />
                      Duplicate
                    </DropdownMenuItem>
                    <DropdownMenuItem>
                      <Download className="mr-2 h-4 w-4" />
                      Download PDF
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      className="text-red-600"
                      onClick={() => deleteResume(resume.id)}
                    >
                      <Trash2 className="mr-2 h-4 w-4" />
                      Delete
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </div>

              <div className="flex h-40 items-center justify-center bg-muted/50">
                <FileText className="h-16 w-16 text-muted-foreground/30" />
              </div>

              <CardContent className="p-4">
                <h3 className="font-semibold truncate">{resume.title}</h3>
                <div className="mt-2 flex items-center gap-2">
                  <Badge
                    className={
                      templateColors[resume.template] || "bg-gray-500/10 text-gray-600"
                    }
                  >
                    {resume.template}
                  </Badge>
                  {resume.isPublic && (
                    <Badge variant="outline" className="text-xs">
                      Public
                    </Badge>
                  )}
                </div>
                <p className="mt-2 text-xs text-muted-foreground">
                  Updated{" "}
                  {resume.updatedAt
                    ? new Date(resume.updatedAt).toLocaleDateString()
                    : new Date(resume.createdAt).toLocaleDateString()}
                </p>
              </CardContent>
            </Card>
          ))}

          {/* Add new resume card */}
          <Card
            className="flex cursor-pointer flex-col items-center justify-center border-dashed transition-all hover:border-primary hover:bg-muted/50 min-h-[280px]"
            onClick={createResume}
          >
            <Plus className="h-10 w-10 text-muted-foreground" />
            <p className="mt-2 font-medium text-muted-foreground">
              New Resume
            </p>
          </Card>
        </div>
      )}
    </div>
  );
}
