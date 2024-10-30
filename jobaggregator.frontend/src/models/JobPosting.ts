export interface JobPosting {
  jobPostingID: number;
  title: string;
  companyName: string;
  location: string;
  description: string;
  postedDate: string;
  applyUrl: string;
  portalID: number;
  externalID: string;
  dateScraped: string;
  hashCode: string;
  salary?: string;
  jobType: string;
  isActive: boolean;
  createdDate: string;
  lastUpdatedDate: string;
  companyLogoUrl?: string;
  portal?: Portal;
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
  