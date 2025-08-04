import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAccount, useDeleteAccount, useSaveAccount } from '../hooks/useAccount';
import type { Account } from '../interfaces/account';

export const AccountForm = () => {
	const { id } = useParams<{ id: string }>();
	const navigate = useNavigate();

	const isNew = id === 'new';
	const accountId = isNew ? null : Number(id);
	const { data: fetchedAccount, isLoading: isFetching } = useAccount(accountId);
	const { mutate, status } = useSaveAccount();
	const deleteMutation = useDeleteAccount();

	const [account, setAccount] = useState<Account>({
		id: 0,
		name: '',
		isActive: false,
		contacts: [],
		vehicles: [],
	});

	useEffect(() => {
		if (fetchedAccount) setAccount(fetchedAccount);
		else if (isNew) {
			setAccount({
				id: 0,
				name: '',
				isActive: false,
				contacts: [],
				vehicles: [],
			});
		}
	}, [fetchedAccount, isNew]);

	const handleDelete = () => {
		if (account.id && confirm('Are you sure you want to delete this account?')) {
			deleteMutation.mutate(account.id, {
				onSuccess: () => navigate('/accounts'),
			});
		}
	};

	const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		const { name, value, type, checked } = e.target;
		setAccount(prev => ({
			...prev,
			[name]: type === 'checkbox' ? checked : value,
		}));
	};

	const handleContactChange = (index: number, e: React.ChangeEvent<HTMLInputElement>) => {
		const { name, value } = e.target;
		setAccount(prev => {
			const contacts = [...prev.contacts];
			contacts[index] = { ...contacts[index], [name]: value };
			return { ...prev, contacts };
		});
	};

	const handleVehiclePhotoChange = (index: number, e: React.ChangeEvent<HTMLInputElement>) => {
		const file = e.target.files?.[0];
		if (!file) return;

		const reader = new FileReader();
		reader.onloadend = () => {
			const base64 = (reader.result as string).split(',')[1];
			setAccount(prev => {
				const vehicles = [...prev.vehicles];
				vehicles[index] = {
					...vehicles[index],
					photo: base64
				};
				return { ...prev, vehicles };
			});
		};
		reader.readAsDataURL(file);
	};

	const addContact = () => {
		setAccount(prev => ({
			...prev,
			contacts: [...prev.contacts, { id: 0, accountId: prev.id, name: '', phone: '', email: '' }],
		}));
	};

	const removeContact = (index: number) => {
		setAccount(prev => {
			const contacts = [...prev.contacts];
			contacts.splice(index, 1);
			return { ...prev, contacts };
		});
	};

	const handleVehicleChange = (index: number, e: React.ChangeEvent<HTMLInputElement>) => {
		const { name, value } = e.target;
		setAccount(prev => {
			const vehicles = [...prev.vehicles];
			vehicles[index] = { ...vehicles[index], [name]: value };
			return { ...prev, vehicles };
		});
	};

	const addVehicle = () => {
		setAccount(prev => ({
			...prev,
			vehicles: [...prev.vehicles, { id: 0, accountId: prev.id, plate: '', model: '', photo: undefined }],
		}));
	};

	const removeVehicle = (index: number) => {
		setAccount(prev => {
			const vehicles = [...prev.vehicles];
			vehicles.splice(index, 1);
			return { ...prev, vehicles };
		});
	};

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		mutate(account, {
			onSuccess: () => navigate('/accounts'),
		});
	};

	if (isFetching || status === 'pending') return <div className="text-center py-5">Loading...</div>;

	return (
		<div className="container-fluid">
			<form onSubmit={handleSubmit} className="bg-white p-4 rounded shadow-sm">
				<h2>{!account.id ? 'Create Account' : 'Edit Account'}</h2>

				<div className="mb-3">
					<label className="form-label">Name</label>
					<input name="name" value={account.name} onChange={handleChange} required className="form-control" />
				</div>

				<div className="form-check mb-3">
					<input
						name="isActive"
						type="checkbox"
						checked={account.isActive}
						onChange={handleChange}
						className="form-check-input"
						id="isActive"
					/>
					<label className="form-check-label" htmlFor="isActive">Active</label>
				</div>

				<hr />
				<h4>Contacts</h4>
				{account.contacts.map((contact, i) => (
					<div key={i} className="row g-2 align-items-center mb-2">
						<div className="col-md-4">
							<input
								name="name"
								placeholder="Name"
								value={contact.name}
								onChange={e => handleContactChange(i, e)}
								required
								className="form-control"
							/>
						</div>
						<div className="col-md-3">
							<input
								name="phone"
								placeholder="Phone"
								value={contact.phone || ''}
								onChange={e => handleContactChange(i, e)}
								className="form-control"
							/>
						</div>
						<div className="col-md-3">
							<input
								name="email"
								placeholder="Email"
								value={contact.email || ''}
								onChange={e => handleContactChange(i, e)}
								type="email"
								className="form-control"
							/>
						</div>
						<div className="col-md-auto">
							<button type="button" onClick={() => removeContact(i)} className="btn btn-outline-danger">
								Remove
							</button>
						</div>
					</div>
				))}
				<button type="button" onClick={addContact} className="btn btn-secondary my-2">
					Add Contact
				</button>

				<hr />
				<h4>Vehicles</h4>
				{account.vehicles.map((vehicle, i) => (
					<div key={i} className="row g-2 align-items-center mb-2">
						<div className="col-md-3">
							<input
								name="plate"
								placeholder="Plate"
								value={vehicle.plate}
								onChange={e => handleVehicleChange(i, e)}
								required
								className="form-control"
							/>
						</div>
						<div className="col-md-3">
							<input
								name="model"
								placeholder="Model"
								value={vehicle.model || ''}
								onChange={e => handleVehicleChange(i, e)}
								className="form-control"
							/>
						</div>
						<div className="col-md-3">
							<input
								type="file"
								accept="image/*"
								onChange={e => handleVehiclePhotoChange(i, e)}
								className="form-control"
							/>
						</div>
						{vehicle.photo && (
							<div className="col-auto">
								<img
									src={`data:image/jpeg;base64,${vehicle.photo}`}
									alt="vehicle"
									style={{ width: 60, height: 40, objectFit: 'cover' }}
								/>
							</div>
						)}
						<div className="col-md-auto">
							<button type="button" onClick={() => removeVehicle(i)} className="btn btn-outline-danger">
								Remove
							</button>
						</div>
					</div>
				))}
				<button type="button" onClick={addVehicle} className="btn btn-secondary my-2">
					Add Vehicle
				</button>

				<hr />
				<div className="d-flex gap-2">
					<button type="submit" className="btn btn-primary">Save</button>
					<button type="button" onClick={() => navigate('/accounts')} className="btn btn-outline-secondary">Cancel</button>
					{!isNew && (
						<button
							type="button"
							onClick={handleDelete}
							disabled={deleteMutation.status === 'pending'}
							className="btn btn-outline-danger ms-auto"
						>
							Delete
						</button>
					)}
				</div>
			</form>
		</div>
	);
};
