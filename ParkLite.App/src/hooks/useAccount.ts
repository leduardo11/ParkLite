import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { Account } from "../interfaces/account";
import { accountService } from "../services/accountService";
import type { PaginatedResult } from "../interfaces/paginatedResult";

export const useAccounts = (page: number, pageSize: number, search: string) =>
	useQuery<PaginatedResult<Account>>({
		queryKey: ['accounts', page, pageSize, search],
		queryFn: () => accountService.getAccountsPaginated(pageSize, (page - 1) * pageSize, search),
	});

export const useAccount = (id?: number | null) =>
	useQuery<Account>({
		queryKey: ['account', id],
		queryFn: () => accountService.getAccount(id!),
		enabled: id != null && id > 0,
	});

export const useSaveAccount = () => {
	const queryClient = useQueryClient();

	return useMutation<Account, Error, Account>({
		mutationFn: async (accountData) => {
			if (accountData.id) {
				return await accountService.updateAccount(accountData.id, accountData);
			} else {
				return await accountService.createAccount(accountData);
			}
		},
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ['accounts'] });
		},
	});
};

export const useDeleteAccount = () => {
	const queryClient = useQueryClient();

	return useMutation<void, Error, number>({
		mutationFn: (id) => accountService.deleteAccount(id),
		onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounts'] }),
	});
};
