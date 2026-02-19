"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import {
  Flag,
  Search,
  Settings,
  Users,
  Percent,
  Shield,
  Check,
  X,
  AlertTriangle,
  Info,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { useFeatureFlagsStore, type FeatureFlag } from "@/store/feature-flags-store";
import { RoleGuard, Permissions } from "@/components/role-guard";

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.05 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 10 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.3 },
  },
};

export default function FeatureFlagsAdminPage() {
  const { flags, toggleFlag } = useFeatureFlagsStore();
  const [searchQuery, setSearchQuery] = useState("");

  const filteredFlags = flags.filter(
    (flag) =>
      flag.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      flag.key.toLowerCase().includes(searchQuery.toLowerCase()) ||
      flag.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const enabledCount = flags.filter((f) => f.enabled).length;
  const roleLimitedCount = flags.filter((f) => f.enabledForRoles?.length).length;
  const percentageRolloutCount = flags.filter(
    (f) => f.percentage !== undefined && f.percentage < 100
  ).length;

  return (
    <RoleGuard
      allowedRoles={Permissions.MANAGE_SETTINGS}
      fallback={
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Shield className="h-12 w-12 text-muted-foreground/50" />
            <h3 className="mt-4 text-lg font-semibold">Access Denied</h3>
            <p className="text-muted-foreground">
              You don&apos;t have permission to manage feature flags.
            </p>
          </CardContent>
        </Card>
      }
    >
      <div className="space-y-6">
        {/* Page header */}
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-3xl font-bold">Feature Flags</h1>
            <p className="text-muted-foreground">
              Control feature availability across the platform
            </p>
          </div>
          <Button>
            <Flag className="mr-2 h-4 w-4" />
            Create Flag
          </Button>
        </div>

        {/* Stats */}
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardContent className="flex items-center gap-4 p-4">
              <div className="rounded-full bg-primary/10 p-2">
                <Flag className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{flags.length}</p>
                <p className="text-sm text-muted-foreground">Total Flags</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="flex items-center gap-4 p-4">
              <div className="rounded-full bg-green-500/10 p-2">
                <Check className="h-5 w-5 text-green-500" />
              </div>
              <div>
                <p className="text-2xl font-bold">{enabledCount}</p>
                <p className="text-sm text-muted-foreground">Enabled</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="flex items-center gap-4 p-4">
              <div className="rounded-full bg-purple-500/10 p-2">
                <Users className="h-5 w-5 text-purple-500" />
              </div>
              <div>
                <p className="text-2xl font-bold">{roleLimitedCount}</p>
                <p className="text-sm text-muted-foreground">Role Limited</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="flex items-center gap-4 p-4">
              <div className="rounded-full bg-amber-500/10 p-2">
                <Percent className="h-5 w-5 text-amber-500" />
              </div>
              <div>
                <p className="text-2xl font-bold">{percentageRolloutCount}</p>
                <p className="text-sm text-muted-foreground">Gradual Rollout</p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Search */}
        <div className="relative max-w-md">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search feature flags..."
            className="pl-9"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        {/* Flags list */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="visible"
          className="space-y-3"
        >
          {filteredFlags.map((flag) => (
            <motion.div key={flag.key} variants={itemVariants}>
              <FeatureFlagCard flag={flag} onToggle={() => toggleFlag(flag.key)} />
            </motion.div>
          ))}
        </motion.div>

        {filteredFlags.length === 0 && (
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <Flag className="h-12 w-12 text-muted-foreground/50" />
              <h3 className="mt-4 text-lg font-semibold">No flags found</h3>
              <p className="text-muted-foreground">
                Try adjusting your search query
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </RoleGuard>
  );
}

function FeatureFlagCard({
  flag,
  onToggle,
}: {
  flag: FeatureFlag;
  onToggle: () => void;
}) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div
              className={cn(
                "rounded-full p-2",
                flag.enabled ? "bg-green-500/10" : "bg-gray-500/10"
              )}
            >
              <Flag
                className={cn(
                  "h-5 w-5",
                  flag.enabled ? "text-green-500" : "text-gray-500"
                )}
              />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-semibold">{flag.name}</h3>
                <code className="rounded bg-muted px-1.5 py-0.5 text-xs">
                  {flag.key}
                </code>
              </div>
              <p className="text-sm text-muted-foreground">{flag.description}</p>

              {/* Badges */}
              <div className="mt-2 flex flex-wrap gap-2">
                {flag.enabledForRoles && flag.enabledForRoles.length > 0 && (
                  <Badge variant="outline" className="text-xs">
                    <Users className="mr-1 h-3 w-3" />
                    {flag.enabledForRoles.join(", ")}
                  </Badge>
                )}
                {flag.percentage !== undefined && flag.percentage < 100 && (
                  <Badge variant="outline" className="text-xs">
                    <Percent className="mr-1 h-3 w-3" />
                    {flag.percentage}% rollout
                  </Badge>
                )}
                {flag.enabledForUsers && flag.enabledForUsers.length > 0 && (
                  <Badge variant="outline" className="text-xs">
                    <Info className="mr-1 h-3 w-3" />
                    {flag.enabledForUsers.length} specific users
                  </Badge>
                )}
              </div>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <Button variant="ghost" size="icon">
              <Settings className="h-4 w-4" />
            </Button>
            <Switch checked={flag.enabled} onCheckedChange={onToggle} />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
