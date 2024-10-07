import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewBusinessCardsComponent } from './view-business-cards.component';

describe('ViewBusinessCardsComponent', () => {
  let component: ViewBusinessCardsComponent;
  let fixture: ComponentFixture<ViewBusinessCardsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewBusinessCardsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewBusinessCardsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
