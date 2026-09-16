export type ContentStatus = "Draft" | "Published" | "Archived";

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type TagDto = {
  id: string;
  name: string;
  slug: string;
};

export type ProjectSummary = {
  id: string;
  slug: string;
  title: string;
  summary: string;
  coverUrl?: string | null;
  status: ContentStatus;
  sortOrder: number;
  publishedAt?: string | null;
  tags: TagDto[];
};

export type ProjectImage = {
  id: string;
  url: string;
  caption?: string | null;
  sortOrder: number;
};

export type ProjectDetail = ProjectSummary & {
  description: string;
  repoUrl?: string | null;
  liveUrl?: string | null;
  images: ProjectImage[];
};

export type ArticleSummary = {
  id: string;
  slug: string;
  title: string;
  summary: string;
  coverUrl?: string | null;
  status: ContentStatus;
  publishedAt?: string | null;
  tags: TagDto[];
};

export type ArticleDetail = ArticleSummary & {
  markdown: string;
  html: string;
};

export type SocialLink = { name: string; url: string };

export type AboutDto = {
  id: string;
  headline: string;
  bioMarkdown: string;
  bioHtml: string;
  skills: string[];
  socialLinks: SocialLink[];
};

export type ContactMessage = {
  id: string;
  name: string;
  email: string;
  subject: string;
  body: string;
  isRead: boolean;
  isReplied: boolean;
  createdAt: string;
  readAt?: string | null;
};

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userName: string;
  role: string;
};

export type MediaAsset = {
  id: string;
  fileName: string;
  url: string;
  contentType: string;
  size: number;
};
