import * as signalR from "@microsoft/signalr";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getAccessToken(): string {
  if (typeof window === "undefined") return "";
  return localStorage.getItem("accessToken") ?? "";
}

function createConnection(hubPath: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${API_URL}${hubPath}`, {
      accessTokenFactory: () => getAccessToken(),
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (ctx) => {
        const delays = [0, 1000, 2000, 5000, 10000, 30000];
        return delays[Math.min(ctx.previousRetryCount, delays.length - 1)];
      },
    })
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

let chatConnection: signalR.HubConnection | null = null;
let notificationConnection: signalR.HubConnection | null = null;
let presenceConnection: signalR.HubConnection | null = null;

export function getChatConnection(): signalR.HubConnection {
  if (!chatConnection) {
    chatConnection = createConnection("/hubs/chat");
  }
  return chatConnection;
}

export function getNotificationConnection(): signalR.HubConnection {
  if (!notificationConnection) {
    notificationConnection = createConnection("/hubs/notifications");
  }
  return notificationConnection;
}

export function getPresenceConnection(): signalR.HubConnection {
  if (!presenceConnection) {
    presenceConnection = createConnection("/hubs/presence");
  }
  return presenceConnection;
}

export async function startConnection(connection: signalR.HubConnection): Promise<void> {
  if (connection.state === signalR.HubConnectionState.Connected) return;
  if (connection.state === signalR.HubConnectionState.Connecting) return;

  try {
    await connection.start();
  } catch (err) {
    console.warn("SignalR connection failed, retrying in 5s...", err);
    setTimeout(() => startConnection(connection), 5000);
  }
}

export async function stopConnection(connection: signalR.HubConnection): Promise<void> {
  if (connection.state === signalR.HubConnectionState.Disconnected) return;
  try {
    await connection.stop();
  } catch {
    // ignore
  }
}

export function stopAllConnections(): void {
  [chatConnection, notificationConnection, presenceConnection].forEach((c) => {
    if (c) stopConnection(c);
  });
  chatConnection = null;
  notificationConnection = null;
  presenceConnection = null;
}

export type { HubConnection } from "@microsoft/signalr";
export { HubConnectionState } from "@microsoft/signalr";
