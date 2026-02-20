"use client";

import { useEffect } from "react";
import { useAuthStore } from "@/store/auth-store";
import { useNotificationStore } from "@/store/notification-store";
import {
  getChatConnection,
  getNotificationConnection,
  getPresenceConnection,
  startConnection,
  stopAllConnections,
} from "@/lib/signalr";

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  const { fetchUnreadCount } = useNotificationStore();

  useEffect(() => {
    if (!isAuthenticated) {
      stopAllConnections();
      return;
    }

    const chatConn = getChatConnection();
    const notifConn = getNotificationConnection();
    const presenceConn = getPresenceConnection();

    startConnection(chatConn);
    startConnection(notifConn);
    startConnection(presenceConn);

    notifConn.on("NewNotification", () => {
      fetchUnreadCount();
    });

    notifConn.on("UnreadCount", (count: number) => {
      // handled by store
    });

    presenceConn.on("UserConnected", () => {});
    presenceConn.on("UserDisconnected", () => {});

    return () => {
      stopAllConnections();
    };
  }, [isAuthenticated, fetchUnreadCount]);

  return <>{children}</>;
}
