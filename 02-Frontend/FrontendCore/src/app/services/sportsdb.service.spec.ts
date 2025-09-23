import { TestBed } from '@angular/core/testing';

import { SportsdbService } from './sportsdb.service';

describe('SportsdbService', () => {
  let service: SportsdbService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SportsdbService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
