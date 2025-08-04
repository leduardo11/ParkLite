import axios from 'axios';
import type { Account } from '../interfaces/account';
import type { PaginatedResult } from '../interfaces/paginatedResult';

const API_URL = `${import.meta.env.VITE_API_URL}/account`;

export const accountRepository = {
	getPaginated: async (limit: number, offset: number, search?: string): Promise<PaginatedResult<Account>> => {
		const res = await axios.get(API_URL, {
			params: { limit, offset, search },
		});
		return res.data;
	},
	getById: async (id: number): Promise<Account> => {
		const res = await axios.get(`${API_URL}/${id}`);
		return res.data?.result ?? res.data ?? {};
	},
	create: async (account: Account): Promise<Account> => {
		const res = await axios.post(API_URL, account);
		return res.data?.result ?? res.data;
	},
	update: async (id: number, account: Account): Promise<Account> => {
		const res = await axios.put(`${API_URL}/${id}`, account);
		return res.data?.result ?? res.data;
	},
	delete: async (id: number): Promise<void> => {
		await axios.delete(`${API_URL}/${id}`);
	},
};
