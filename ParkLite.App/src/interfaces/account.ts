import type { Contact } from "./contact";
import type { Vehicle } from "./vehicle";

export interface Account {
	id: number;
	name: string;
	isActive: boolean;
	contacts: Contact[];
	vehicles: Vehicle[];
}
