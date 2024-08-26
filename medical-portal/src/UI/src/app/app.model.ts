export enum SubmittalStatusEnum {
  'OpenRequired' = 'Open-Required',
  'Issued' = 'Issued',
  'Sent' = 'Sent',
}

export enum CaseStageEnum {
  'Opened' = 'Opened',
  'OpenPendingSubmission' = 'Open Pending Submission',
  'UnderReview' = 'Under Review',
  'FileEndTasks' = 'File End Tasks',
  'IntakeValidation' = 'Intake Validation',
  'Closed' = 'Closed',
}

export enum DocumentTypeEnum {
  'DMER' = 'DMER',
}

export enum DMERStatusEnum {
  'RequiredClaimed' = 'Required - Claimed',
  'RequiredUnclaimed' = 'Required - Unclaimed',
  'NotRequested' = 'Not Requested',
  'NonComplyUnclaimed' = 'Non-Comply - Unclaimed',
  'NonComplyClaimed' = 'Non-Comply - Claimed'
}

export enum LicenceStatusCode {
  Active = 'ACTIVE',
  Inactive = 'INACTIVE',
  Terminated = 'TERMINATED',
}

export enum SESSION_STORAGE_KEYS {
  PROFILE = 'profile'
}
