"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeft,
  Save,
  Download,
  Undo2,
  Redo2,
  Type,
  Square,
  Circle,
  Triangle,
  Image,
  Minus,
  Plus,
  Trash2,
  Copy,
  Layers,
  Palette,
  MousePointer,
  PenTool,
  Star,
  ArrowRight,
  Diamond,
  Hexagon,
  ZoomIn,
  ZoomOut,
  AlignLeft,
  AlignCenter,
  AlignRight,
  Bold,
  Italic,
  Underline,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Slider } from "@/components/ui/slider";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { designApi } from "@/lib/api";
import { cn } from "@/lib/utils";

type ToolType = "select" | "text" | "shapes" | "draw" | "image";

interface DesignData {
  id: string;
  name: string;
  width: number;
  height: number;
  canvasJson: string;
  thumbnail: string;
}

export default function DesignEditorPage() {
  const params = useParams();
  const router = useRouter();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const fabricCanvasRef = useRef<any>(null);
  const [design, setDesign] = useState<DesignData | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [activeTool, setActiveTool] = useState<ToolType>("select");
  const [designName, setDesignName] = useState("");
  const [zoom, setZoom] = useState(100);
  const [fabricLoaded, setFabricLoaded] = useState(false);
  const [selectedObject, setSelectedObject] = useState<any>(null);
  const [fillColor, setFillColor] = useState("#4f46e5");
  const [strokeColor, setStrokeColor] = useState("#000000");
  const [fontSize, setFontSize] = useState(24);
  const [history, setHistory] = useState<string[]>([]);
  const [historyIndex, setHistoryIndex] = useState(-1);

  useEffect(() => {
    loadDesign();
  }, [params.id]);

  useEffect(() => {
    if (design && canvasRef.current && !fabricCanvasRef.current) {
      initFabric();
    }
  }, [design, fabricLoaded]);

  const loadDesign = async () => {
    try {
      const { data } = await designApi.getById(params.id as string);
      setDesign(data);
      setDesignName(data.name);
    } catch {
      router.push("/dashboard/designs");
    } finally {
      setLoading(false);
    }
  };

  const initFabric = async () => {
    if (fabricCanvasRef.current) return;

    try {
      const fabric = await import("fabric");
      const canvas = new fabric.Canvas(canvasRef.current!, {
        width: design!.width,
        height: design!.height,
        backgroundColor: "#ffffff",
        selection: true,
      });

      fabricCanvasRef.current = canvas;

      if (design!.canvasJson && design!.canvasJson !== "{}") {
        try {
          await canvas.loadFromJSON(design!.canvasJson);
          canvas.renderAll();
        } catch {
          // Invalid JSON, start fresh
        }
      }

      canvas.on("selection:created", (e: any) => {
        setSelectedObject(e.selected?.[0] || null);
        setActiveTool("select");
      });
      canvas.on("selection:updated", (e: any) => {
        setSelectedObject(e.selected?.[0] || null);
      });
      canvas.on("selection:cleared", () => {
        setSelectedObject(null);
      });
      canvas.on("object:modified", () => {
        saveToHistory();
      });

      saveToHistory();
      setFabricLoaded(true);
    } catch (err) {
      console.error("Failed to initialize Fabric.js:", err);
    }
  };

  const saveToHistory = () => {
    if (!fabricCanvasRef.current) return;
    const json = JSON.stringify(fabricCanvasRef.current.toJSON());
    setHistory((prev) => {
      const newHistory = prev.slice(0, historyIndex + 1);
      newHistory.push(json);
      return newHistory;
    });
    setHistoryIndex((prev) => prev + 1);
  };

  const undo = async () => {
    if (historyIndex <= 0 || !fabricCanvasRef.current) return;
    const newIndex = historyIndex - 1;
    await fabricCanvasRef.current.loadFromJSON(history[newIndex]);
    fabricCanvasRef.current.renderAll();
    setHistoryIndex(newIndex);
  };

  const redo = async () => {
    if (historyIndex >= history.length - 1 || !fabricCanvasRef.current) return;
    const newIndex = historyIndex + 1;
    await fabricCanvasRef.current.loadFromJSON(history[newIndex]);
    fabricCanvasRef.current.renderAll();
    setHistoryIndex(newIndex);
  };

  const saveDesign = async () => {
    if (!fabricCanvasRef.current || !design) return;
    setSaving(true);
    try {
      const canvasJson = JSON.stringify(fabricCanvasRef.current.toJSON());
      const thumbnail = fabricCanvasRef.current.toDataURL({
        format: "png",
        quality: 0.3,
        multiplier: 0.2,
      });
      await designApi.update(design.id, {
        name: designName,
        canvasJson,
        thumbnail,
      });
    } catch (err) {
      console.error("Failed to save design:", err);
    } finally {
      setSaving(false);
    }
  };

  const addText = async () => {
    if (!fabricCanvasRef.current) return;
    const fabric = await import("fabric");
    const text = new fabric.IText("Double-click to edit", {
      left: 100,
      top: 100,
      fontSize,
      fill: fillColor,
      fontFamily: "Arial",
    });
    fabricCanvasRef.current.add(text);
    fabricCanvasRef.current.setActiveObject(text);
    fabricCanvasRef.current.renderAll();
    saveToHistory();
  };

  const addShape = async (type: string) => {
    if (!fabricCanvasRef.current) return;
    const fabric = await import("fabric");
    let shape: any;

    switch (type) {
      case "rectangle":
        shape = new fabric.Rect({
          left: 100, top: 100, width: 200, height: 150,
          fill: fillColor, stroke: strokeColor, strokeWidth: 2,
          rx: 8, ry: 8,
        });
        break;
      case "circle":
        shape = new fabric.Circle({
          left: 100, top: 100, radius: 80,
          fill: fillColor, stroke: strokeColor, strokeWidth: 2,
        });
        break;
      case "triangle":
        shape = new fabric.Triangle({
          left: 100, top: 100, width: 180, height: 160,
          fill: fillColor, stroke: strokeColor, strokeWidth: 2,
        });
        break;
      case "line":
        shape = new fabric.Line([100, 100, 400, 100], {
          stroke: strokeColor, strokeWidth: 3,
        });
        break;
      case "star":
        shape = new fabric.Polygon(createStarPoints(5, 80, 40), {
          left: 100, top: 100,
          fill: fillColor, stroke: strokeColor, strokeWidth: 2,
        });
        break;
      case "arrow":
        shape = new fabric.Path(
          "M 0 20 L 80 20 L 80 0 L 120 30 L 80 60 L 80 40 L 0 40 Z",
          {
            left: 100, top: 100,
            fill: fillColor, stroke: strokeColor, strokeWidth: 1,
            scaleX: 1.5, scaleY: 1.5,
          }
        );
        break;
      default:
        shape = new fabric.Rect({
          left: 100, top: 100, width: 200, height: 150,
          fill: fillColor, stroke: strokeColor, strokeWidth: 2,
        });
    }

    fabricCanvasRef.current.add(shape);
    fabricCanvasRef.current.setActiveObject(shape);
    fabricCanvasRef.current.renderAll();
    saveToHistory();
  };

  const createStarPoints = (
    spikes: number,
    outerRadius: number,
    innerRadius: number
  ) => {
    const points = [];
    let rot = (Math.PI / 2) * 3;
    const step = Math.PI / spikes;
    for (let i = 0; i < spikes; i++) {
      points.push({ x: outerRadius * Math.cos(rot), y: outerRadius * Math.sin(rot) });
      rot += step;
      points.push({ x: innerRadius * Math.cos(rot), y: innerRadius * Math.sin(rot) });
      rot += step;
    }
    return points;
  };

  const deleteSelected = () => {
    if (!fabricCanvasRef.current) return;
    const active = fabricCanvasRef.current.getActiveObjects();
    if (active.length) {
      active.forEach((obj: any) => fabricCanvasRef.current.remove(obj));
      fabricCanvasRef.current.discardActiveObject();
      fabricCanvasRef.current.renderAll();
      saveToHistory();
    }
  };

  const duplicateSelected = async () => {
    if (!fabricCanvasRef.current) return;
    const active = fabricCanvasRef.current.getActiveObject();
    if (!active) return;
    const cloned = await active.clone();
    cloned.set({ left: active.left + 20, top: active.top + 20 });
    fabricCanvasRef.current.add(cloned);
    fabricCanvasRef.current.setActiveObject(cloned);
    fabricCanvasRef.current.renderAll();
    saveToHistory();
  };

  const handleZoom = (newZoom: number) => {
    if (!fabricCanvasRef.current) return;
    const zoomFactor = newZoom / 100;
    fabricCanvasRef.current.setZoom(zoomFactor);
    fabricCanvasRef.current.renderAll();
    setZoom(newZoom);
  };

  const addImage = () => {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/*";
    input.onchange = async (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (!file || !fabricCanvasRef.current) return;
      const reader = new FileReader();
      reader.onload = async (event) => {
        const fabric = await import("fabric");
        const img = await fabric.FabricImage.fromURL(event.target?.result as string);
        const maxSize = 400;
        if (img.width && img.height) {
          const scale = Math.min(maxSize / img.width, maxSize / img.height);
          img.scale(scale);
        }
        img.set({ left: 100, top: 100 });
        fabricCanvasRef.current.add(img);
        fabricCanvasRef.current.setActiveObject(img);
        fabricCanvasRef.current.renderAll();
        saveToHistory();
      };
      reader.readAsDataURL(file);
    };
    input.click();
  };

  const exportDesign = () => {
    if (!fabricCanvasRef.current) return;
    const dataURL = fabricCanvasRef.current.toDataURL({
      format: "png",
      quality: 1,
      multiplier: 2,
    });
    const link = document.createElement("a");
    link.download = `${designName || "design"}.png`;
    link.href = dataURL;
    link.click();
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  return (
    <div className="flex h-[calc(100vh-5rem)] flex-col -m-4 lg:-m-6">
      {/* Toolbar */}
      <div className="flex items-center justify-between border-b bg-card px-4 py-2">
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.push("/dashboard/designs")}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <Input
            value={designName}
            onChange={(e) => setDesignName(e.target.value)}
            className="w-48 h-8 text-sm"
            placeholder="Design name"
          />
          {design && (
            <Badge variant="outline" className="text-xs">
              {design.width} x {design.height}
            </Badge>
          )}
        </div>

        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" onClick={undo} disabled={historyIndex <= 0}>
            <Undo2 className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="icon" onClick={redo} disabled={historyIndex >= history.length - 1}>
            <Redo2 className="h-4 w-4" />
          </Button>
          <Separator orientation="vertical" className="mx-1 h-6" />
          <div className="flex items-center gap-1">
            <Button variant="ghost" size="icon" onClick={() => handleZoom(Math.max(10, zoom - 10))}>
              <ZoomOut className="h-4 w-4" />
            </Button>
            <span className="text-xs w-10 text-center">{zoom}%</span>
            <Button variant="ghost" size="icon" onClick={() => handleZoom(Math.min(400, zoom + 10))}>
              <ZoomIn className="h-4 w-4" />
            </Button>
          </div>
          <Separator orientation="vertical" className="mx-1 h-6" />
          <Button variant="ghost" size="icon" onClick={exportDesign}>
            <Download className="h-4 w-4" />
          </Button>
          <Button size="sm" onClick={saveDesign} disabled={saving}>
            <Save className="mr-2 h-4 w-4" />
            {saving ? "Saving..." : "Save"}
          </Button>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        {/* Left Tool Sidebar */}
        <div className="flex w-14 flex-col items-center gap-1 border-r bg-card py-2">
          {[
            { tool: "select" as ToolType, icon: MousePointer, label: "Select" },
            { tool: "text" as ToolType, icon: Type, label: "Text" },
            { tool: "shapes" as ToolType, icon: Square, label: "Shapes" },
            { tool: "draw" as ToolType, icon: PenTool, label: "Draw" },
            { tool: "image" as ToolType, icon: Image, label: "Image" },
          ].map(({ tool, icon: Icon, label }) => (
            <Button
              key={tool}
              variant={activeTool === tool ? "default" : "ghost"}
              size="icon"
              className="h-10 w-10"
              onClick={() => {
                setActiveTool(tool);
                if (tool === "text") addText();
                if (tool === "image") addImage();
              }}
              title={label}
            >
              <Icon className="h-5 w-5" />
            </Button>
          ))}
          <Separator className="my-1 w-8" />
          <Button variant="ghost" size="icon" className="h-10 w-10" onClick={duplicateSelected} title="Duplicate">
            <Copy className="h-5 w-5" />
          </Button>
          <Button variant="ghost" size="icon" className="h-10 w-10 text-red-500" onClick={deleteSelected} title="Delete">
            <Trash2 className="h-5 w-5" />
          </Button>
        </div>

        {/* Shape panel (when shapes tool is active) */}
        {activeTool === "shapes" && (
          <div className="w-48 border-r bg-card p-3 overflow-y-auto">
            <p className="mb-2 text-xs font-semibold uppercase text-muted-foreground">
              Shapes
            </p>
            <div className="grid grid-cols-3 gap-2">
              {[
                { type: "rectangle", icon: Square, label: "Rect" },
                { type: "circle", icon: Circle, label: "Circle" },
                { type: "triangle", icon: Triangle, label: "Triangle" },
                { type: "line", icon: Minus, label: "Line" },
                { type: "star", icon: Star, label: "Star" },
                { type: "arrow", icon: ArrowRight, label: "Arrow" },
              ].map(({ type, icon: Icon, label }) => (
                <button
                  key={type}
                  className="flex flex-col items-center gap-1 rounded-lg p-2 text-xs hover:bg-accent transition-colors"
                  onClick={() => addShape(type)}
                >
                  <Icon className="h-6 w-6" />
                  {label}
                </button>
              ))}
            </div>

            <Separator className="my-3" />

            <p className="mb-2 text-xs font-semibold uppercase text-muted-foreground">
              Fill Color
            </p>
            <div className="flex items-center gap-2">
              <input
                type="color"
                value={fillColor}
                onChange={(e) => {
                  setFillColor(e.target.value);
                  if (selectedObject && fabricCanvasRef.current) {
                    selectedObject.set("fill", e.target.value);
                    fabricCanvasRef.current.renderAll();
                  }
                }}
                className="h-8 w-8 cursor-pointer rounded border-0"
              />
              <Input
                value={fillColor}
                onChange={(e) => setFillColor(e.target.value)}
                className="h-8 text-xs"
              />
            </div>

            <p className="mb-2 mt-3 text-xs font-semibold uppercase text-muted-foreground">
              Stroke Color
            </p>
            <div className="flex items-center gap-2">
              <input
                type="color"
                value={strokeColor}
                onChange={(e) => {
                  setStrokeColor(e.target.value);
                  if (selectedObject && fabricCanvasRef.current) {
                    selectedObject.set("stroke", e.target.value);
                    fabricCanvasRef.current.renderAll();
                  }
                }}
                className="h-8 w-8 cursor-pointer rounded border-0"
              />
              <Input
                value={strokeColor}
                onChange={(e) => setStrokeColor(e.target.value)}
                className="h-8 text-xs"
              />
            </div>
          </div>
        )}

        {/* Properties panel */}
        {selectedObject && activeTool !== "shapes" && (
          <div className="w-56 border-r bg-card p-3 overflow-y-auto">
            <p className="mb-3 text-xs font-semibold uppercase text-muted-foreground">
              Properties
            </p>

            <div className="space-y-3">
              <div>
                <label className="text-xs text-muted-foreground">Fill</label>
                <div className="flex items-center gap-2 mt-1">
                  <input
                    type="color"
                    value={fillColor}
                    onChange={(e) => {
                      setFillColor(e.target.value);
                      selectedObject.set("fill", e.target.value);
                      fabricCanvasRef.current?.renderAll();
                    }}
                    className="h-8 w-8 cursor-pointer rounded border-0"
                  />
                  <Input value={fillColor} onChange={(e) => setFillColor(e.target.value)} className="h-8 text-xs" />
                </div>
              </div>

              <div>
                <label className="text-xs text-muted-foreground">Opacity</label>
                <Slider
                  defaultValue={[(selectedObject?.opacity || 1) * 100]}
                  max={100}
                  step={1}
                  onValueChange={([v]) => {
                    selectedObject.set("opacity", v / 100);
                    fabricCanvasRef.current?.renderAll();
                  }}
                  className="mt-1"
                />
              </div>

              {selectedObject?.type?.includes("text") && (
                <>
                  <div>
                    <label className="text-xs text-muted-foreground">Font Size</label>
                    <Input
                      type="number"
                      value={selectedObject.fontSize || fontSize}
                      onChange={(e) => {
                        const size = parseInt(e.target.value) || 24;
                        setFontSize(size);
                        selectedObject.set("fontSize", size);
                        fabricCanvasRef.current?.renderAll();
                      }}
                      className="h-8 text-xs mt-1"
                    />
                  </div>
                  <div className="flex gap-1">
                    <Button variant="ghost" size="icon" className="h-8 w-8"
                      onClick={() => {
                        selectedObject.set("fontWeight", selectedObject.fontWeight === "bold" ? "normal" : "bold");
                        fabricCanvasRef.current?.renderAll();
                      }}>
                      <Bold className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" className="h-8 w-8"
                      onClick={() => {
                        selectedObject.set("fontStyle", selectedObject.fontStyle === "italic" ? "normal" : "italic");
                        fabricCanvasRef.current?.renderAll();
                      }}>
                      <Italic className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" className="h-8 w-8"
                      onClick={() => {
                        selectedObject.set("underline", !selectedObject.underline);
                        fabricCanvasRef.current?.renderAll();
                      }}>
                      <Underline className="h-4 w-4" />
                    </Button>
                  </div>
                </>
              )}
            </div>
          </div>
        )}

        {/* Canvas area */}
        <div className="flex-1 overflow-auto bg-muted/30 flex items-center justify-center p-8">
          <div className="shadow-2xl">
            <canvas ref={canvasRef} />
          </div>
        </div>
      </div>
    </div>
  );
}
