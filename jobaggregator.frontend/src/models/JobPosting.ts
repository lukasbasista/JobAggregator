export interface JobPosting {
  jobPostingID: number;
  title: string;
  companyName: string;
  currency: string;
  location: string;
  description: string;
  applyUrl: string;
  portalID: number;
  externalID: string;
  dateScraped: string;
  hashCode: string;
  salaryFrom?: number;
  salaryTo?: number;
  jobType: string;
  isActive: boolean;
  createdDate: string;
  lastUpdatedDate: string;
  portal?: Portal;
  company?: Company;
}

export interface Portal {
  portalID: number;
  portalName: string;
  baseUrl: string;
  isActive: boolean;
  createdDate: string;
  lastUpdatedDate: string;
  portalLogoUrl?: string;
}
  

export interface Company {
  companyID: number;
  companyName: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  createdDate: string;
  lastUpdatedDate: string;
  foundedYear?: string;
  headquarters?: string;
  industry?: string;
  numberOfEmployees?: string;

}