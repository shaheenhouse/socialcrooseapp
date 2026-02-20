import { create } from 'zustand';
import { khataApi } from '@/lib/api';

interface KhataParty {
  id: string;
  partyName: string;
  partyPhone?: string;
  partyAddress?: string;
  type: 'customer' | 'supplier';
  totalCredit: number;
  totalDebit: number;
  balance: number;
  lastTransactionAt?: string;
  createdAt: string;
}

interface KhataEntry {
  id: string;
  amount: number;
  type: 'credit' | 'debit';
  description: string;
  date: string;
  attachmentUrl?: string;
  runningBalance: number;
  createdAt: string;
}

interface KhataSummary {
  totalReceivable: number;
  totalPayable: number;
  netBalance: number;
  totalParties: number;
  totalTransactions: number;
}

interface KhataState {
  parties: KhataParty[];
  selectedParty: KhataParty | null;
  entries: KhataEntry[];
  summary: KhataSummary | null;
  isLoading: boolean;
  error: string | null;

  fetchParties: (params?: { search?: string; status?: string }) => Promise<void>;
  fetchPartyById: (id: string) => Promise<void>;
  createParty: (data: { partyName: string; partyPhone?: string; partyAddress?: string; type: 'customer' | 'supplier'; openingBalance?: number }) => Promise<void>;
  fetchEntries: (partyId: string, params?: { page?: number; type?: string; from?: string; to?: string }) => Promise<void>;
  addEntry: (partyId: string, data: { amount: number; type: 'credit' | 'debit'; description: string; date?: string }) => Promise<void>;
  fetchSummary: (params?: { from?: string; to?: string }) => Promise<void>;
  sendReminder: (partyId: string) => Promise<void>;
  clearError: () => void;
}

export const useKhataStore = create<KhataState>()((set) => ({
  parties: [],
  selectedParty: null,
  entries: [],
  summary: null,
  isLoading: false,
  error: null,

  fetchParties: async (params) => {
    set({ isLoading: true, error: null });
    try {
      const response = await khataApi.getAll(params);
      set({ parties: response.data.items ?? response.data, isLoading: false });
    } catch (e: any) {
      set({ isLoading: false, error: e.response?.data?.error || 'Failed to fetch khata records' });
    }
  },

  fetchPartyById: async (id) => {
    set({ isLoading: true });
    try {
      const response = await khataApi.getById(id);
      set({ selectedParty: response.data, isLoading: false });
    } catch (e: any) {
      set({ isLoading: false, error: e.response?.data?.error || 'Failed to fetch party' });
    }
  },

  createParty: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const response = await khataApi.create(data);
      set((state) => ({
        parties: [response.data, ...state.parties],
        isLoading: false,
      }));
    } catch (e: any) {
      set({ isLoading: false, error: e.response?.data?.error || 'Failed to create party' });
      throw e;
    }
  },

  fetchEntries: async (partyId, params) => {
    set({ isLoading: true });
    try {
      const response = await khataApi.getEntries(partyId, params);
      set({ entries: response.data.items ?? response.data, isLoading: false });
    } catch (e: any) {
      set({ isLoading: false, error: e.response?.data?.error || 'Failed to fetch entries' });
    }
  },

  addEntry: async (partyId, data) => {
    set({ isLoading: true, error: null });
    try {
      const response = await khataApi.addEntry(partyId, data);
      set((state) => ({
        entries: [response.data, ...state.entries],
        isLoading: false,
      }));
    } catch (e: any) {
      set({ isLoading: false, error: e.response?.data?.error || 'Failed to add entry' });
      throw e;
    }
  },

  fetchSummary: async (params) => {
    try {
      const response = await khataApi.getSummary(params);
      set({ summary: response.data });
    } catch { /* silent */ }
  },

  sendReminder: async (partyId) => {
    await khataApi.sendReminder(partyId);
  },

  clearError: () => set({ error: null }),
}));
