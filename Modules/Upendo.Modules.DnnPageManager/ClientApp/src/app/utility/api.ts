export function getApiName(propertyName: string): string {
  switch (propertyName) {
    case 'Priority':
      return 'SetPagePriority';
    case 'Keywords':
      return 'SetPageKeywords';
    case 'Description':
      return 'SetPageDescription';
    case 'Title':
      return 'SetPageTitle';
    case 'Name':
      return 'SetPageName';
    case 'PrimaryURL':
      return 'SetPagePrimaryURL';
    default:
      return 'String';
  }
}

export function getParameterName(propertyName: string): string {
  switch (propertyName) {
    case 'Priority':
      return 'priority';
    case 'Keywords':
      return 'keywords';
    case 'Description':
      return 'description';
    case 'Title':
      return 'title';
    case 'Name':
      return 'name';
    case 'PrimaryURL':
      return 'url';
    default:
      return 'String';
  }
}
