import type { Account } from "../interfaces/account";
import type { PaginatedResult } from "../interfaces/paginatedResult";
import { accountRepository } from "../repository/accountRepository";

export const accountService = {
	async getAccountsPaginated(limit: number, offset: number, search?: string): Promise<PaginatedResult<Account>> {
		return await accountRepository.getPaginated(limit, offset, search);
	},
	async getAccount(id: number) {
		return await accountRepository.getById(id);
	},
	async createAccount(account: Account) {
		return await accountRepository.create(account);
	},
	async updateAccount(id: number, account: Account) {
		return await accountRepository.update(id, account);
	},
	async deleteAccount(id: number) {
		return await accountRepository.delete(id);
	},
};
