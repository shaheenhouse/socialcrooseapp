"use client";

import { useEffect, useRef, useCallback, useState } from "react";
import type { HubConnection } from "@microsoft/signalr";
import { HubConnectionState } from "@microsoft/signalr";
import {
  getChatConnection,
  getNotificationConnection,
  getPresenceConnection,
  startConnection,
} from "@/lib/signalr";
import { useAuthStore } from "@/store/auth-store";

export function useSignalR(
  hub: "chat" | "notifications" | "presence"
) {
  const { isAuthenticated } = useAuthStore();
  const connectionRef = useRef<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    if (!isAuthenticated) return;

    const getConn =
      hub === "chat"
        ? getChatConnection
        : hub === "notifications"
        ? getNotificationConnection
        : getPresenceConnection;

    const conn = getConn();
    connectionRef.current = conn;

    const onReconnected = () => setConnected(true);
    const onClose = () => setConnected(false);

    conn.onreconnected(onReconnected);
    conn.onclose(onClose);

    startConnection(conn).then(() => {
      if (conn.state === HubConnectionState.Connected) {
        setConnected(true);
      }
    });

    return () => {
      conn.off("reconnected", onReconnected);
    };
  }, [hub, isAuthenticated]);

  const invoke = useCallback(
    async (method: string, ...args: unknown[]) => {
      const conn = connectionRef.current;
      if (conn?.state === HubConnectionState.Connected) {
        return conn.invoke(method, ...args);
      }
    },
    []
  );

  const on = useCallback(
    (event: string, callback: (...args: any[]) => void) => {
      connectionRef.current?.on(event, callback);
      return () => connectionRef.current?.off(event, callback);
    },
    []
  );

  return { connection: connectionRef.current, connected, invoke, on };
}

export function useChatHub() {
  return useSignalR("chat");
}

export function useNotificationHub() {
  return useSignalR("notifications");
}

export function usePresenceHub() {
  return useSignalR("presence");
}
