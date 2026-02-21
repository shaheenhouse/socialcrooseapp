"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import { portfolioApi } from "@/lib/api";
import { PublicPortfolio } from "@/components/portfolio/public-portfolio";
import type { Portfolio } from "@/types/portfolio";

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

export default function PortfolioPage() {
  const params = useParams();
  const slug = params.slug as string;

  const [portfolio, setPortfolio] = useState<Portfolio | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    if (!slug) return;

    (async () => {
      try {
        const { data } = await portfolioApi.getBySlug(slug);
        if (!data) {
          setNotFound(true);
          return;
        }
        const parsed = parsePortfolioData(data);
        setPortfolio(parsed);

        document.title = `${parsed.personalInfo.fullName || slug} - ${parsed.personalInfo.title || "Portfolio"}`;
        
        const metaDesc = document.querySelector('meta[name="description"]');
        if (metaDesc) {
          metaDesc.setAttribute("content", parsed.personalInfo.bio?.slice(0, 160) || "");
        } else {
          const meta = document.createElement("meta");
          meta.name = "description";
          meta.content = parsed.personalInfo.bio?.slice(0, 160) || "";
          document.head.appendChild(meta);
        }
      } catch {
        setNotFound(true);
      } finally {
        setLoading(false);
      }
    })();
  }, [slug]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-center space-y-4">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto" />
          <p className="text-muted-foreground">Loading portfolio...</p>
        </div>
      </div>
    );
  }

  if (notFound || !portfolio) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-center space-y-4">
          <h1 className="text-4xl font-bold">Portfolio Not Found</h1>
          <p className="text-muted-foreground">
            The portfolio you&apos;re looking for doesn&apos;t exist or is set to private.
          </p>
          <a
            href="/"
            className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Go Home
          </a>
        </div>
      </div>
    );
  }

  return <PublicPortfolio portfolio={portfolio} />;
}
