"use client";

import { useEffect, useRef } from "react";
import { useAuthStore } from "@/store/auth-store";
import { useNotificationStore } from "@/store/notification-store";
import {
  getChatConnection,
  getNotificationConnection,
  getPresenceConnection,
  startConnection,
  stopAllConnectionsAsync,
} from "@/lib/signalr";

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuthStore();
  const fetchUnreadCount = useNotificationStore((s) => s.fetchUnreadCount);
  const fetchUnreadRef = useRef(fetchUnreadCount);
  fetchUnreadRef.current = fetchUnreadCount;

  useEffect(() => {
    if (!isAuthenticated) {
      void stopAllConnectionsAsync();
      return;
    }

    const chatConn = getChatConnection();
    const notifConn = getNotificationConnection();
    const presenceConn = getPresenceConnection();

    notifConn.on("NewNotification", () => {
      void fetchUnreadRef.current();
    });
    notifConn.on("UnreadCount", () => {});
    presenceConn.on("UserConnected", () => {});
    presenceConn.on("UserDisconnected", () => {});

    let cancelled = false;
    const isCancelled = () => cancelled;

    void (async () => {
      const opts = { isCancelled };
      await startConnection(chatConn, opts);
      if (isCancelled()) return;
      await startConnection(notifConn, opts);
      if (isCancelled()) return;
      await startConnection(presenceConn, opts);
    })();

    return () => {
      cancelled = true;
      void stopAllConnectionsAsync();
    };
  }, [isAuthenticated]);

  return <>{children}</>;
}
