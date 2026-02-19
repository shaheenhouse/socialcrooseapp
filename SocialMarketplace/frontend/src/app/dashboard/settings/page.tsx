"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import {
  User,
  Mail,
  Phone,
  Lock,
  Bell,
  Globe,
  Shield,
  CreditCard,
  Smartphone,
  Moon,
  Sun,
  Eye,
  EyeOff,
  Save,
  Camera,
  Trash2,
  AlertTriangle,
  Check,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { useTheme } from "next-themes";

const languages = [
  { code: "en", name: "English" },
  { code: "ur", name: "Urdu" },
  { code: "ar", name: "Arabic" },
  { code: "zh", name: "Chinese" },
  { code: "es", name: "Spanish" },
];

const timezones = [
  { value: "PKT", label: "Pakistan Standard Time (PKT)" },
  { value: "IST", label: "Indian Standard Time (IST)" },
  { value: "GMT", label: "Greenwich Mean Time (GMT)" },
  { value: "EST", label: "Eastern Standard Time (EST)" },
  { value: "PST", label: "Pacific Standard Time (PST)" },
];

export default function SettingsPage() {
  const { theme, setTheme } = useTheme();
  const [showPassword, setShowPassword] = useState(false);
  const [notifications, setNotifications] = useState({
    email: true,
    push: true,
    orders: true,
    messages: true,
    marketing: false,
    security: true,
  });

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h1 className="text-3xl font-bold">Settings</h1>
        <p className="text-muted-foreground">
          Manage your account settings and preferences
        </p>
      </div>

      <Tabs defaultValue="profile" className="w-full">
        <TabsList className="grid w-full grid-cols-2 lg:w-auto lg:grid-cols-5">
          <TabsTrigger value="profile">Profile</TabsTrigger>
          <TabsTrigger value="security">Security</TabsTrigger>
          <TabsTrigger value="notifications">Notifications</TabsTrigger>
          <TabsTrigger value="preferences">Preferences</TabsTrigger>
          <TabsTrigger value="billing">Billing</TabsTrigger>
        </TabsList>

        {/* Profile Settings */}
        <TabsContent value="profile" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Profile Information</CardTitle>
              <CardDescription>
                Update your profile details and public information
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Avatar */}
              <div className="flex items-center gap-4">
                <Avatar className="h-20 w-20">
                  <AvatarImage src="https://api.dicebear.com/7.x/avataaars/svg?seed=Ahmed" />
                  <AvatarFallback>AK</AvatarFallback>
                </Avatar>
                <div className="space-y-2">
                  <Button variant="outline">
                    <Camera className="mr-2 h-4 w-4" />
                    Change Photo
                  </Button>
                  <p className="text-xs text-muted-foreground">
                    JPG, PNG or GIF. Max size 2MB.
                  </p>
                </div>
              </div>

              <Separator />

              {/* Name fields */}
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm font-medium">First Name</label>
                  <Input defaultValue="Ahmed" />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">Last Name</label>
                  <Input defaultValue="Khan" />
                </div>
              </div>

              {/* Username */}
              <div className="space-y-2">
                <label className="text-sm font-medium">Username</label>
                <div className="flex items-center gap-2">
                  <span className="text-muted-foreground">@</span>
                  <Input defaultValue="ahmedkhan" />
                </div>
                <p className="text-xs text-muted-foreground">
                  Your public profile URL: marketplace.com/@ahmedkhan
                </p>
              </div>

              {/* Email */}
              <div className="space-y-2">
                <label className="text-sm font-medium">Email</label>
                <div className="flex items-center gap-2">
                  <Input defaultValue="ahmed@example.com" type="email" />
                  <Badge variant="outline" className="shrink-0 text-green-500">
                    <Check className="mr-1 h-3 w-3" />
                    Verified
                  </Badge>
                </div>
              </div>

              {/* Phone */}
              <div className="space-y-2">
                <label className="text-sm font-medium">Phone</label>
                <div className="flex items-center gap-2">
                  <Input defaultValue="+92 300 1234567" type="tel" />
                  <Badge variant="outline" className="shrink-0 text-green-500">
                    <Check className="mr-1 h-3 w-3" />
                    Verified
                  </Badge>
                </div>
              </div>

              {/* Bio */}
              <div className="space-y-2">
                <label className="text-sm font-medium">Bio</label>
                <textarea
                  className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  defaultValue="Full-stack developer with 5+ years of experience..."
                />
                <p className="text-xs text-muted-foreground">
                  Brief description for your profile. Max 500 characters.
                </p>
              </div>
            </CardContent>
            <CardFooter className="flex justify-end gap-2">
              <Button variant="outline">Cancel</Button>
              <Button>
                <Save className="mr-2 h-4 w-4" />
                Save Changes
              </Button>
            </CardFooter>
          </Card>

          {/* Location */}
          <Card>
            <CardHeader>
              <CardTitle>Location</CardTitle>
              <CardDescription>
                Set your location and timezone
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Country</label>
                  <Select defaultValue="pk">
                    <SelectTrigger>
                      <SelectValue placeholder="Select country" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="pk">Pakistan</SelectItem>
                      <SelectItem value="in">India</SelectItem>
                      <SelectItem value="us">United States</SelectItem>
                      <SelectItem value="uk">United Kingdom</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">City</label>
                  <Input defaultValue="Lahore" />
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Timezone</label>
                <Select defaultValue="PKT">
                  <SelectTrigger>
                    <SelectValue placeholder="Select timezone" />
                  </SelectTrigger>
                  <SelectContent>
                    {timezones.map((tz) => (
                      <SelectItem key={tz.value} value={tz.value}>
                        {tz.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
            <CardFooter className="flex justify-end">
              <Button>Save Location</Button>
            </CardFooter>
          </Card>
        </TabsContent>

        {/* Security Settings */}
        <TabsContent value="security" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Password</CardTitle>
              <CardDescription>
                Change your password to keep your account secure
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Current Password</label>
                <div className="relative">
                  <Input
                    type={showPassword ? "text" : "password"}
                    placeholder="Enter current password"
                  />
                  <Button
                    variant="ghost"
                    size="icon"
                    className="absolute right-0 top-0"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">New Password</label>
                <Input type="password" placeholder="Enter new password" />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Confirm New Password</label>
                <Input type="password" placeholder="Confirm new password" />
              </div>
            </CardContent>
            <CardFooter className="flex justify-end">
              <Button>Update Password</Button>
            </CardFooter>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Two-Factor Authentication</CardTitle>
              <CardDescription>
                Add an extra layer of security to your account
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="rounded-full bg-green-500/10 p-3">
                    <Smartphone className="h-6 w-6 text-green-500" />
                  </div>
                  <div>
                    <p className="font-medium">Authenticator App</p>
                    <p className="text-sm text-muted-foreground">
                      Use an authenticator app for 2FA
                    </p>
                  </div>
                </div>
                <Switch defaultChecked />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Active Sessions</CardTitle>
              <CardDescription>
                Manage your active sessions across devices
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between rounded-lg border p-4">
                <div className="flex items-center gap-4">
                  <div className="rounded-full bg-primary/10 p-2">
                    <Globe className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <p className="font-medium">Chrome on Windows</p>
                    <p className="text-sm text-muted-foreground">
                      Lahore, Pakistan • Current session
                    </p>
                  </div>
                </div>
                <Badge variant="secondary">Active</Badge>
              </div>
              <div className="flex items-center justify-between rounded-lg border p-4">
                <div className="flex items-center gap-4">
                  <div className="rounded-full bg-muted p-2">
                    <Smartphone className="h-5 w-5" />
                  </div>
                  <div>
                    <p className="font-medium">Safari on iPhone</p>
                    <p className="text-sm text-muted-foreground">
                      Lahore, Pakistan • 2 hours ago
                    </p>
                  </div>
                </div>
                <Button variant="ghost" size="sm">
                  Revoke
                </Button>
              </div>
            </CardContent>
            <CardFooter>
              <Button variant="destructive" className="w-full">
                Log Out All Other Sessions
              </Button>
            </CardFooter>
          </Card>
        </TabsContent>

        {/* Notification Settings */}
        <TabsContent value="notifications" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Notification Preferences</CardTitle>
              <CardDescription>
                Choose how you want to receive notifications
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <Mail className="h-5 w-5 text-muted-foreground" />
                  <div>
                    <p className="font-medium">Email Notifications</p>
                    <p className="text-sm text-muted-foreground">
                      Receive notifications via email
                    </p>
                  </div>
                </div>
                <Switch
                  checked={notifications.email}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, email: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <Bell className="h-5 w-5 text-muted-foreground" />
                  <div>
                    <p className="font-medium">Push Notifications</p>
                    <p className="text-sm text-muted-foreground">
                      Receive push notifications on your device
                    </p>
                  </div>
                </div>
                <Switch
                  checked={notifications.push}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, push: checked })
                  }
                />
              </div>

              <Separator />

              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">Order Updates</p>
                  <p className="text-sm text-muted-foreground">
                    Get notified about your order status
                  </p>
                </div>
                <Switch
                  checked={notifications.orders}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, orders: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">Messages</p>
                  <p className="text-sm text-muted-foreground">
                    Get notified about new messages
                  </p>
                </div>
                <Switch
                  checked={notifications.messages}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, messages: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">Security Alerts</p>
                  <p className="text-sm text-muted-foreground">
                    Get notified about security events
                  </p>
                </div>
                <Switch
                  checked={notifications.security}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, security: checked })
                  }
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium">Marketing</p>
                  <p className="text-sm text-muted-foreground">
                    Receive promotional emails and offers
                  </p>
                </div>
                <Switch
                  checked={notifications.marketing}
                  onCheckedChange={(checked) =>
                    setNotifications({ ...notifications, marketing: checked })
                  }
                />
              </div>
            </CardContent>
            <CardFooter className="flex justify-end">
              <Button>Save Preferences</Button>
            </CardFooter>
          </Card>
        </TabsContent>

        {/* Preferences Settings */}
        <TabsContent value="preferences" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Appearance</CardTitle>
              <CardDescription>
                Customize how the app looks
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  {theme === "dark" ? (
                    <Moon className="h-5 w-5 text-muted-foreground" />
                  ) : (
                    <Sun className="h-5 w-5 text-muted-foreground" />
                  )}
                  <div>
                    <p className="font-medium">Theme</p>
                    <p className="text-sm text-muted-foreground">
                      Select your preferred theme
                    </p>
                  </div>
                </div>
                <Select
                  value={theme}
                  onValueChange={(value) => setTheme(value)}
                >
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Select theme" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="light">Light</SelectItem>
                    <SelectItem value="dark">Dark</SelectItem>
                    <SelectItem value="system">System</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Language & Region</CardTitle>
              <CardDescription>
                Set your language and regional preferences
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Language</label>
                <Select defaultValue="en">
                  <SelectTrigger>
                    <SelectValue placeholder="Select language" />
                  </SelectTrigger>
                  <SelectContent>
                    {languages.map((lang) => (
                      <SelectItem key={lang.code} value={lang.code}>
                        {lang.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium">Currency</label>
                <Select defaultValue="PKR">
                  <SelectTrigger>
                    <SelectValue placeholder="Select currency" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="PKR">Pakistani Rupee (PKR)</SelectItem>
                    <SelectItem value="USD">US Dollar (USD)</SelectItem>
                    <SelectItem value="EUR">Euro (EUR)</SelectItem>
                    <SelectItem value="GBP">British Pound (GBP)</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
            <CardFooter className="flex justify-end">
              <Button>Save Preferences</Button>
            </CardFooter>
          </Card>

          {/* Delete Account */}
          <Card className="border-destructive/50">
            <CardHeader>
              <CardTitle className="text-destructive">Danger Zone</CardTitle>
              <CardDescription>
                Irreversible and destructive actions
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between rounded-lg border border-destructive/50 bg-destructive/5 p-4">
                <div className="flex items-center gap-4">
                  <AlertTriangle className="h-5 w-5 text-destructive" />
                  <div>
                    <p className="font-medium">Delete Account</p>
                    <p className="text-sm text-muted-foreground">
                      Permanently delete your account and all data
                    </p>
                  </div>
                </div>
                <Button variant="destructive">
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Billing Settings */}
        <TabsContent value="billing" className="mt-6 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Current Plan</CardTitle>
              <CardDescription>
                Manage your subscription and billing
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="rounded-lg border bg-gradient-to-br from-primary/10 via-purple-500/10 to-pink-500/10 p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <Badge className="mb-2">Pro Plan</Badge>
                    <p className="text-2xl font-bold">$29/month</p>
                    <p className="text-sm text-muted-foreground">
                      Billed monthly • Next billing: Jan 1, 2026
                    </p>
                  </div>
                  <Button variant="outline">Upgrade Plan</Button>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Payment Methods</CardTitle>
              <CardDescription>
                Manage your payment methods
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between rounded-lg border p-4">
                <div className="flex items-center gap-4">
                  <CreditCard className="h-5 w-5" />
                  <div>
                    <p className="font-medium">Visa ending in 4242</p>
                    <p className="text-sm text-muted-foreground">
                      Expires 12/2026
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant="secondary">Default</Badge>
                  <Button variant="ghost" size="sm">
                    Edit
                  </Button>
                </div>
              </div>
              <Button variant="outline" className="w-full">
                <CreditCard className="mr-2 h-4 w-4" />
                Add Payment Method
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Billing History</CardTitle>
              <CardDescription>
                View your past invoices
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div
                    key={i}
                    className="flex items-center justify-between border-b pb-4 last:border-0 last:pb-0"
                  >
                    <div>
                      <p className="font-medium">Pro Plan - Monthly</p>
                      <p className="text-sm text-muted-foreground">
                        {i === 1 ? "Dec" : i === 2 ? "Nov" : "Oct"} 1, 2025
                      </p>
                    </div>
                    <div className="flex items-center gap-4">
                      <span className="font-medium">$29.00</span>
                      <Button variant="ghost" size="sm">
                        Download
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
