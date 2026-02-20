"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  Palette,
  Plus,
  MoreVertical,
  Copy,
  Trash2,
  Edit,
  Image,
  FileText,
  LayoutGrid,
  Presentation,
  Square,
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { designApi } from "@/lib/api";

interface DesignItem {
  id: string;
  name: string;
  description?: string;
  width: number;
  height: number;
  thumbnail: string;
  status: string;
  category: string;
  isTemplate: boolean;
  isPublic: boolean;
  createdAt: string;
  updatedAt?: string;
}

const DESIGN_SIZES = [
  { name: "Instagram Post", width: 1080, height: 1080, icon: Image },
  { name: "Instagram Story", width: 1080, height: 1920, icon: Image },
  { name: "Facebook Post", width: 940, height: 788, icon: Image },
  { name: "Twitter Post", width: 1200, height: 675, icon: Image },
  { name: "YouTube Thumbnail", width: 1280, height: 720, icon: Image },
  { name: "LinkedIn Banner", width: 1584, height: 396, icon: LayoutGrid },
  { name: "Presentation", width: 1920, height: 1080, icon: Presentation },
  { name: "A4 Document", width: 2480, height: 3508, icon: FileText },
  { name: "Resume", width: 2480, height: 3508, icon: FileText },
  { name: "Business Card", width: 1050, height: 600, icon: Square },
  { name: "Custom", width: 800, height: 600, icon: Square },
];

export default function DesignsPage() {
  const router = useRouter();
  const [designs, setDesigns] = useState<DesignItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [showSizeDialog, setShowSizeDialog] = useState(false);

  useEffect(() => {
    loadDesigns();
  }, []);

  const loadDesigns = async () => {
    try {
      const { data } = await designApi.getMe();
      setDesigns(data.data || []);
    } catch {
      setDesigns([]);
    } finally {
      setLoading(false);
    }
  };

  const createDesign = async (
    name: string,
    width: number,
    height: number,
    category: string = "custom"
  ) => {
    try {
      const { data } = await designApi.create({
        name,
        width,
        height,
        category,
      });
      router.push(`/dashboard/designs/${data.id}`);
    } catch (err) {
      console.error("Failed to create design:", err);
    }
  };

  const duplicateDesign = async (id: string) => {
    try {
      await designApi.duplicate(id);
      await loadDesigns();
    } catch (err) {
      console.error("Failed to duplicate design:", err);
    }
  };

  const deleteDesign = async (id: string) => {
    try {
      await designApi.delete(id);
      setDesigns(designs.filter((d) => d.id !== id));
    } catch (err) {
      console.error("Failed to delete design:", err);
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
          <h1 className="text-3xl font-bold">Design Studio</h1>
          <p className="text-muted-foreground">
            Create stunning designs for social media, presentations, and more
          </p>
        </div>
      </div>

      <Tabs defaultValue="designs">
        <TabsList>
          <TabsTrigger value="designs">My Designs</TabsTrigger>
          <TabsTrigger value="create">Create New</TabsTrigger>
          <TabsTrigger value="templates">Templates</TabsTrigger>
        </TabsList>

        <TabsContent value="designs" className="mt-6">
          {designs.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-20">
                <Palette className="h-16 w-16 text-muted-foreground/50" />
                <h2 className="mt-4 text-xl font-semibold">No Designs Yet</h2>
                <p className="mt-2 text-center text-muted-foreground max-w-md">
                  Create your first design using our powerful editor with shapes,
                  text, images, flowcharts, and AI-powered tools.
                </p>
                <Button
                  className="mt-6"
                  onClick={() =>
                    createDesign("My First Design", 1080, 1080, "social-media")
                  }
                >
                  <Plus className="mr-2 h-4 w-4" />
                  Create Your First Design
                </Button>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {designs.map((design) => (
                <Card
                  key={design.id}
                  className="group relative cursor-pointer overflow-hidden transition-all hover:shadow-lg"
                  onClick={() => router.push(`/dashboard/designs/${design.id}`)}
                >
                  <div className="absolute right-2 top-2 z-10 opacity-0 transition-opacity group-hover:opacity-100">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button
                          variant="secondary"
                          size="icon"
                          className="h-8 w-8"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem
                          onClick={(e) => {
                            e.stopPropagation();
                            router.push(`/dashboard/designs/${design.id}`);
                          }}
                        >
                          <Edit className="mr-2 h-4 w-4" />
                          Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={(e) => {
                            e.stopPropagation();
                            duplicateDesign(design.id);
                          }}
                        >
                          <Copy className="mr-2 h-4 w-4" />
                          Duplicate
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="text-red-600"
                          onClick={(e) => {
                            e.stopPropagation();
                            deleteDesign(design.id);
                          }}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Delete
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>

                  <div className="flex h-44 items-center justify-center bg-muted/50">
                    {design.thumbnail ? (
                      <img
                        src={design.thumbnail}
                        alt={design.name}
                        className="h-full w-full object-contain"
                      />
                    ) : (
                      <Palette className="h-12 w-12 text-muted-foreground/30" />
                    )}
                  </div>

                  <CardContent className="p-3">
                    <h3 className="font-medium truncate text-sm">
                      {design.name}
                    </h3>
                    <div className="mt-1 flex items-center gap-2">
                      <span className="text-xs text-muted-foreground">
                        {design.width} x {design.height}
                      </span>
                      <Badge variant="outline" className="text-xs">
                        {design.status}
                      </Badge>
                    </div>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {design.updatedAt
                        ? new Date(design.updatedAt).toLocaleDateString()
                        : new Date(design.createdAt).toLocaleDateString()}
                    </p>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="create" className="mt-6">
          <div className="space-y-6">
            <h2 className="text-xl font-semibold">Choose a Size</h2>
            <div className="grid gap-3 md:grid-cols-3 lg:grid-cols-4">
              {DESIGN_SIZES.map((size) => (
                <Card
                  key={size.name}
                  className="cursor-pointer transition-all hover:border-primary hover:shadow-md"
                  onClick={() =>
                    createDesign(
                      size.name,
                      size.width,
                      size.height,
                      size.name.toLowerCase().replace(/[^a-z0-9]/g, "-")
                    )
                  }
                >
                  <CardContent className="flex items-center gap-4 p-4">
                    <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                      <size.icon className="h-6 w-6 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium">{size.name}</p>
                      <p className="text-sm text-muted-foreground">
                        {size.width} x {size.height}px
                      </p>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="templates" className="mt-6">
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-20">
              <LayoutGrid className="h-16 w-16 text-muted-foreground/50" />
              <h2 className="mt-4 text-xl font-semibold">Templates Coming Soon</h2>
              <p className="mt-2 text-center text-muted-foreground max-w-md">
                Professional design templates will be available here. For now,
                start from scratch and let your creativity flow!
              </p>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
