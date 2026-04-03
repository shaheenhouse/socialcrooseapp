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
    .configureLogging(
      process.env.NODE_ENV === "development"
        ? signalR.LogLevel.None
        : signalR.LogLevel.Warning
    )
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

/** Strict Mode cleanup calls stop() while start() is still running — expected, not worth retrying. */
function isBenignStartupError(err: unknown): boolean {
  if (err instanceof DOMException && err.name === "AbortError") return true;
  if (err instanceof Error && err.name === "AbortError") return true;
  const msg = err instanceof Error ? err.message : String(err);
  return (
    /stopped during negotiation/i.test(msg) ||
    /before stop\(\) was called/i.test(msg) ||
    /Failed to start the HttpConnection/i.test(msg)
  );
}

export async function startConnection(
  connection: signalR.HubConnection,
  options?: { isCancelled?: () => boolean }
): Promise<void> {
  if (options?.isCancelled?.()) return;
  if (connection.state === signalR.HubConnectionState.Connected) return;
  if (connection.state === signalR.HubConnectionState.Connecting) return;

  try {
    await connection.start();
  } catch (err) {
    if (options?.isCancelled?.()) return;
    // React Strict Mode / navigation — do not retry
    if (isBenignStartupError(err)) return;
    console.warn("SignalR connection failed, retrying in 5s...", err);
    setTimeout(() => startConnection(connection, options), 5000);
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

/** Clears singletons first so new mounts get fresh connections; then stops old instances. */
export async function stopAllConnectionsAsync(): Promise<void> {
  const chat = chatConnection;
  const notif = notificationConnection;
  const pres = presenceConnection;
  chatConnection = null;
  notificationConnection = null;
  presenceConnection = null;
  await Promise.all(
    [chat, notif, pres].filter(Boolean).map((c) => stopConnection(c!))
  );
}

/** @deprecated Prefer stopAllConnectionsAsync so stops can complete before reconnecting. */
export function stopAllConnections(): void {
  void stopAllConnectionsAsync();
}

export type { HubConnection } from "@microsoft/signalr";
export { HubConnectionState } from "@microsoft/signalr";
