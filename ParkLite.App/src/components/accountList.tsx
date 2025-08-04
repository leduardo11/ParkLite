import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAccounts } from '../hooks/useAccount';

export const AccountList = () => {
	const [page, setPage] = useState(1);
	const [search, setSearch] = useState('');
	const [debouncedSearch, setDebouncedSearch] = useState('');
	const pageSize = 10;
	const navigate = useNavigate();

	useEffect(() => {
		const handler = setTimeout(() => {
			setDebouncedSearch(search);
			setPage(1);
		}, 1000);
		return () => clearTimeout(handler);
	}, [search]);

	const { data, isLoading, isError } = useAccounts(page, pageSize, debouncedSearch);
	const accounts = data?.result ?? [];
	const total = data?.total ?? 0;
	const totalPages = Math.ceil(total / pageSize);

	if (isLoading) return <div className="text-center py-5">Loading...</div>;
	if (isError) return <div className="text-danger text-center py-5">Failed to load accounts.</div>;

	return (
		<div className="container-fluid py-4">
			<div className="row mb-3 align-items-center">
				<div className="col">
					<h2>Accounts</h2>
				</div>
				<div className="col-auto">
					<button className="btn btn-primary" onClick={() => navigate('/accounts/new')}>
						Add New Account
					</button>
				</div>
			</div>
			<div className="row mb-3">
				<div className="col-md-4 col-sm-6 col-12">
					<input
						type="text"
						placeholder="Search accounts..."
						value={search}
						onChange={e => setSearch(e.target.value)}
						className="form-control"
					/>
				</div>
			</div>

			<table className="table table-bordered table-striped table-hover bg-white">
				<thead className="table-light">
					<tr>
						<th>ID</th>
						<th>Name</th>
						<th>Active</th>
						<th>Contacts</th>
						<th>Vehicles</th>
						<th>Actions</th>
					</tr>
				</thead>
				<tbody>
					{accounts.map(account => (
						<tr key={account.id}>
							<td data-label="ID">{account.id}</td>
							<td data-label="Name">{account.name}</td>
							<td data-label="Active">{account.isActive ? 'Yes' : 'No'}</td>
							<td data-label="Contacts">{account.contacts.length}</td>
							<td data-label="Vehicles">{account.vehicles.length}</td>
							<td data-label="Actions">
								<Link to={`/accounts/${account.id}`} className="btn btn-sm btn-outline-secondary">
									Edit
								</Link>
							</td>
						</tr>
					))}
				</tbody>
			</table>

			<div className="d-flex justify-content-center align-items-center gap-3 mt-3">
				<button
					className="btn btn-secondary"
					disabled={page === 1}
					onClick={() => setPage(p => p - 1)}
				>
					Prev
				</button>

				<span>
					Page {page} of {totalPages}
				</span>

				<button
					className="btn btn-secondary"
					disabled={page === totalPages || totalPages === 0}
					onClick={() => setPage(p => p + 1)}
				>
					Next
				</button>
			</div>
		</div>
	);
};
