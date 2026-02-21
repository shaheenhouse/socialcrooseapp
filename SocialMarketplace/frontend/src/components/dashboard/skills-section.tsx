"use client";

import { useState } from "react";
import { Skill, Role } from "@/types/portfolio";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";
import { Plus, X } from "lucide-react";

interface SkillsSectionProps {
  skills: Skill[];
  roles: Role[];
  onUpdateSkills: (skills: Skill[]) => void;
  onUpdateRoles: (roles: Role[]) => void;
}

const skillCategories = [
  "Programming Languages",
  "Frameworks & Libraries",
  "Databases",
  "Cloud & DevOps",
  "Tools & Software",
  "Soft Skills",
  "Other",
];

const levelColors: Record<string, string> = {
  expert: "bg-emerald-100 text-emerald-800 border-emerald-300 dark:bg-emerald-900/50 dark:text-emerald-300",
  proficient: "bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-900/50 dark:text-blue-300",
  intermediate: "bg-amber-100 text-amber-800 border-amber-300 dark:bg-amber-900/50 dark:text-amber-300",
  beginner: "bg-gray-100 text-gray-800 border-gray-300 dark:bg-gray-900/50 dark:text-gray-300",
};

export function SkillsSection({ skills, roles, onUpdateSkills, onUpdateRoles }: SkillsSectionProps) {
  const [newSkill, setNewSkill] = useState({ name: "", level: "proficient" as Skill["level"], category: "Programming Languages" });
  const [newRole, setNewRole] = useState({ title: "", level: "expert" as Role["level"] });

  const handleAddSkill = () => {
    if (!newSkill.name.trim()) return;
    const skill: Skill = { ...newSkill, id: Date.now().toString() };
    onUpdateSkills([...skills, skill]);
    setNewSkill({ name: "", level: "proficient", category: "Programming Languages" });
    toast.success("Skill added!");
  };

  const handleDeleteSkill = (id: string) => {
    onUpdateSkills(skills.filter(s => s.id !== id));
  };

  const handleAddRole = () => {
    if (!newRole.title.trim()) return;
    const role: Role = { ...newRole, id: Date.now().toString() };
    onUpdateRoles([...roles, role]);
    setNewRole({ title: "", level: "expert" });
    toast.success("Role added!");
  };

  const handleDeleteRole = (id: string) => {
    onUpdateRoles(roles.filter(r => r.id !== id));
  };

  const skillsByCategory = skills.reduce((acc, skill) => {
    if (!acc[skill.category]) {
      acc[skill.category] = [];
    }
    acc[skill.category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  return (
    <div className="space-y-6">
      {/* Roles Section */}
      <Card>
        <CardHeader>
          <CardTitle>Professional Roles</CardTitle>
          <CardDescription>Add your professional roles and expertise areas</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-2">
            <Input
              value={newRole.title}
              onChange={(e) => setNewRole({ ...newRole, title: e.target.value })}
              placeholder="Full Stack Developer, DevOps Engineer, etc."
              className="flex-1"
              onKeyDown={(e) => e.key === "Enter" && handleAddRole()}
            />
            <Select
              value={newRole.level}
              onValueChange={(value) => setNewRole({ ...newRole, level: value as Role["level"] })}
            >
              <SelectTrigger className="w-[140px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="expert">Expert</SelectItem>
                <SelectItem value="proficient">Proficient</SelectItem>
                <SelectItem value="intermediate">Intermediate</SelectItem>
                <SelectItem value="beginner">Beginner</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleAddRole}>
              <Plus className="h-4 w-4" />
            </Button>
          </div>

          {roles.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {roles.map((role) => (
                <Badge
                  key={role.id}
                  className={`py-2 px-3 text-sm flex items-center gap-2 border ${levelColors[role.level] || ""}`}
                >
                  <span>{role.title}</span>
                  <span className="text-xs opacity-70">({role.level})</span>
                  <button onClick={() => handleDeleteRole(role.id)} className="ml-1">
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Skills Section */}
      <Card>
        <CardHeader>
          <CardTitle>Technical Skills</CardTitle>
          <CardDescription>Add your technical skills by category</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col md:flex-row gap-2">
            <Input
              value={newSkill.name}
              onChange={(e) => setNewSkill({ ...newSkill, name: e.target.value })}
              placeholder="React, Python, Docker, etc."
              className="flex-1"
              onKeyDown={(e) => e.key === "Enter" && handleAddSkill()}
            />
            <Select
              value={newSkill.category}
              onValueChange={(value) => setNewSkill({ ...newSkill, category: value })}
            >
              <SelectTrigger className="w-[200px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {skillCategories.map((cat) => (
                  <SelectItem key={cat} value={cat}>{cat}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={newSkill.level}
              onValueChange={(value) => setNewSkill({ ...newSkill, level: value as Skill["level"] })}
            >
              <SelectTrigger className="w-[140px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="expert">Expert</SelectItem>
                <SelectItem value="proficient">Proficient</SelectItem>
                <SelectItem value="intermediate">Intermediate</SelectItem>
                <SelectItem value="beginner">Beginner</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleAddSkill}>
              <Plus className="h-4 w-4" />
            </Button>
          </div>

          {Object.keys(skillsByCategory).length > 0 ? (
            <div className="space-y-4 mt-4">
              {Object.entries(skillsByCategory).map(([category, categorySkills]) => (
                <div key={category}>
                  <h4 className="text-sm font-medium text-muted-foreground mb-2">{category}</h4>
                  <div className="flex flex-wrap gap-2">
                    {categorySkills.map((skill) => (
                      <Badge
                        key={skill.id}
                        className={`py-1.5 px-3 border ${levelColors[skill.level] || ""}`}
                      >
                        {skill.name}
                        <button onClick={() => handleDeleteSkill(skill.id)} className="ml-2">
                          <X className="h-3 w-3" />
                        </button>
                      </Badge>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-muted-foreground text-center py-4">
              No skills added yet. Start adding your technical skills.
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
